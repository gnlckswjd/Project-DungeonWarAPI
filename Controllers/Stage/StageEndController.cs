using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ZLogger;

namespace DungeonWarAPI.Controllers.Stage;

[Route("[controller]")]
[ApiController]
public class StageEndController : ControllerBase
{
    private readonly IStageDataCRUD _stageDataCRUD;
    private readonly IItemDATACRUD _itemDataCRUD;
    private readonly IUserDataCRUD _userDataCRUD;
    private readonly MasterDataProvider _masterDataProvider;
    private readonly IMemoryDatabase _memoryDatabase;
    private readonly ILogger<StageEndController> _logger;

    public StageEndController(ILogger<StageEndController> logger, IMemoryDatabase memoryDatabase,
        MasterDataProvider masterDataProvider,
        IStageDataCRUD stageDataCRUD, IItemDATACRUD itemDataCRUD, IUserDataCRUD userDataCrud)
    {
        _memoryDatabase = memoryDatabase;
        _stageDataCRUD = stageDataCRUD;
        _itemDataCRUD = itemDataCRUD;
        _userDataCRUD = userDataCrud;
        _masterDataProvider = masterDataProvider;
        _logger = logger;
    }

    [HttpPost]
    public async Task<StageEndResponse> Post(StageEndRequest request)
    {
        var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new StageEndResponse();
        var gameUserId = userAuthAndState.GameUserId;
        var state = userAuthAndState.State;
        var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);


        var (errorCode, itemCodeAndCount, npcCodeAndCount, stageLevel) = await LoadStageDataAsync(key, gameUserId, state);
        if (errorCode != ErrorCode.None)
        {
	        response.Error = errorCode;
	        return response;
        }

		var stageNpcList = _masterDataProvider.GetStageNpcList(stageLevel);

        var (isCleared, earnedExp) = StageRequestVerifier.VerifyClearAndCalcExp(stageNpcList, npcCodeAndCount);
        if (isCleared == false)
        {
            await _memoryDatabase.DeleteStageDataAsync(key);
            response.IsCleared = isCleared;
            response.Error = ErrorCode.None;
            return response;
        }

        var userKey = MemoryDatabaseKeyGenerator.MakeUIDKey(request.Email);

        errorCode = await _memoryDatabase.UpdateUserStateAsync(userKey, userAuthAndState, UserStateCode.Lobby);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        errorCode = await ProcessStageCompletionAsync(gameUserId, stageLevel, earnedExp, itemCodeAndCount);
        if (errorCode != ErrorCode.None)
        {
            await _memoryDatabase.UpdateUserStateAsync(userKey, userAuthAndState, UserStateCode.InStage);
            response.Error = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, StageLevel = stageLevel },
	        "StageEnd Success");

		await _memoryDatabase.DeleteStageDataAsync(key);
        response.IsCleared = isCleared;
        response.Error = ErrorCode.None;
        return response;
    }

    private async Task<(ErrorCode, List<(Int32, Int32)>, List<(Int32, Int32)>, Int32)> LoadStageDataAsync(String key, Int32 gameUserId, UserStateCode state)
    {
        if (state != UserStateCode.InStage)
        {
            _logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.WrongUserState, GameUserId = gameUserId },
                "CheckStageAccessibility");

            return (ErrorCode.WrongUserState, default, default, 0);
        }

        var (errorCode, dictionary) = await _memoryDatabase.LoadStageDataAsync(key);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, default, default, 0);
        }
		var(itemCodeAndCount, npcCodeAndCount, stageLevel) =StageDataParser.ParseStageData(dictionary);

		return (ErrorCode.None, itemCodeAndCount, npcCodeAndCount, stageLevel);
    }

    private async Task<ErrorCode> ProcessStageCompletionAsync(Int32 gameUserId, Int32 stageLevel, Int32 earnedExp, List<(Int32, Int32)> itemCodeAndCount)
    {
        var (errorCode, isIncrement) = await _stageDataCRUD.IncreaseMaxClearedStageAsync(gameUserId, stageLevel);
        if (errorCode != ErrorCode.None)
        {
            return errorCode;
        }

        (errorCode, Int32 existingLevel, Int32 existingExp) = await _userDataCRUD.UpdateExpAsync(gameUserId, earnedExp);
        if (errorCode != ErrorCode.None)
        {
            await _stageDataCRUD.RollbackIncreaseMaxClearedStageAsync(gameUserId, isIncrement);

            return errorCode;
        }

        errorCode = await _itemDataCRUD.InsertItemsAsync(gameUserId, itemCodeAndCount);
        if (errorCode != ErrorCode.None)
        {
            await _stageDataCRUD.RollbackUpdateExpAsync(gameUserId, existingLevel, existingExp);
            await _stageDataCRUD.RollbackIncreaseMaxClearedStageAsync(gameUserId, isIncrement);

            return errorCode;
        }

        return ErrorCode.None;
    }

}