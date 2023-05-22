using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers.Stage;

[Route("[controller]")]
[ApiController]
public class ItemAcquisitionController : ControllerBase
{
	private readonly MasterDataProvider _masterDataProvider;
    private readonly IMemoryDatabase _memoryDatabase;
    private readonly ILogger<ItemAcquisitionController> _logger;

    public ItemAcquisitionController(ILogger<ItemAcquisitionController> logger, IMemoryDatabase memoryDatabase, MasterDataProvider masterDataProvider)
    {
        _memoryDatabase = memoryDatabase;
        _masterDataProvider = masterDataProvider;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ItemAcquisitionResponse> Post(ItemAcquisitionRequest request)
    {
        var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new ItemAcquisitionResponse();

        if (authenticatedUserState == null)
        {
	        response.Error = ErrorCode.WrongAuthenticatedUserState;
	        return response;
        }

        var gameUserId = authenticatedUserState.GameUserId;
        var itemCode = request.ItemCode;
        var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

        var (errorCode, currentItemCount, maxItemCount) = await LoadAcquisitionAndMaxItemCountAsync(key, itemCode, gameUserId, authenticatedUserState.State);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        (errorCode, var totalAcquisitionCount) = StageRequestVerifier.VerifyItemCount(itemCode, currentItemCount, request.ItemCount, maxItemCount);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        errorCode = await _memoryDatabase.IncrementItemCountAsync(key, itemCode, totalAcquisitionCount);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }


		_logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, ItemCode = request.ItemCode, CurrentItemCount = totalAcquisitionCount },
			"ItemAcquisition Success");

		response.Error = ErrorCode.None;
        return response;
    }

    private async Task<(ErrorCode, Int32 currentItemCount, Int32 maxItemSpawnCount)> LoadAcquisitionAndMaxItemCountAsync(String key, Int32 itemCode, Int32 gameUserId, UserStateCode state)
    {
        if (state != UserStateCode.InStage)
        {
            _logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.WrongUserState, GameUserId = gameUserId },
                "CheckStageAccessibility");

            return (ErrorCode.WrongUserState, 0, 0);
        }

        var (errorCode, stageLevel) = await _memoryDatabase.LoadStageLevelAsync(key);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, 0, 0);
        }

        var stageItem = _masterDataProvider.GetStageItemByStageAndCode(stageLevel, itemCode);
        if (stageItem == null)
        {
            return (ErrorCode.WrongItemCode, 0, 0);
        }

        (errorCode, var currentItemCount) = await _memoryDatabase.LoadItemAcquisitionCountAsync(key, itemCode);
        if (errorCode != ErrorCode.None)
        {
            return (errorCode, 0, 0);
        }

        return (ErrorCode.None, currentItemCount, stageItem.ItemCount);
    }
}