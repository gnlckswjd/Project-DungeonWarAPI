using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.Database.Game;
using ZLogger;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using DungeonWarAPI.Models.DAO.Redis;

namespace DungeonWarAPI.Controllers.Stage;

[Route("[controller]")]
[ApiController]
public class StageStartController : ControllerBase
{
    private readonly IDungeonStageService _dungeonStageService;
    private readonly MasterDataManager _masterDataManager;
    private readonly IMemoryDatabase _memoryDatabase;
    private readonly ILogger<StageStartController> _logger;

    public StageStartController(ILogger<StageStartController> logger, IMemoryDatabase memoryDatabase,
        MasterDataManager masterDataManager,
        IDungeonStageService dungeonStageService)
    {
        _memoryDatabase = memoryDatabase;
        _dungeonStageService = dungeonStageService;
        _masterDataManager = masterDataManager;
        _logger = logger;
    }

    [HttpPost]
    public async Task<StageStartResponse> Post(StageStartRequest request)
    {
        var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
        var response = new StageStartResponse();
        var gameUserId = userAuthAndState.GameUserId;
        var selectedStageLevel = request.SelectedStageLevel;

        var errorCode = await CheckStageAccessibilityAsync(gameUserId, selectedStageLevel, userAuthAndState.State);
        if (errorCode != 0)
        {
            response.Error = errorCode;
            return response;
        }

        var itemList = _masterDataManager.GetStageItemList(selectedStageLevel);
        var npcList = _masterDataManager.GetStageNpcList(selectedStageLevel);
        if (!itemList.Any() || !npcList.Any())
        {
            response.Error = ErrorCode.WrongStageLevel;
            return response;
        }

        errorCode = await StoreInitialStageDataAsync(userAuthAndState, itemList, npcList, selectedStageLevel);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        response.ItemList = itemList;
        response.NpcList = npcList.ToList();
        response.Error = ErrorCode.None;
        return response;
    }

    private async Task<ErrorCode> CheckStageAccessibilityAsync(int gameUserId, int selectedStageLevel,
        UserStateCode state)
    {
        if (state != UserStateCode.Lobby)
        {
            _logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.WrongUserState, GameUserId = gameUserId },
                "CheckStageAccessibility");
            return ErrorCode.WrongUserState;
        }

        var (errorCode, maxClearedStage) = await _dungeonStageService.LoadStageListAsync(gameUserId);
        if (errorCode != ErrorCode.None)
        {
            return errorCode;
        }

        errorCode = StageInitializer.CheckAccessibility(maxClearedStage, selectedStageLevel);
        if (errorCode != ErrorCode.None)
        {
            return errorCode;
        }

        return errorCode;
    }

    private async Task<ErrorCode> StoreInitialStageDataAsync(UserAuthAndState userAuthAndState,
        List<StageItem> itemList, List<StageNpc> npcList, int selectedStageLevel)
    {
        var stageKey = MemoryDatabaseKeyGenerator.MakeStageKey(userAuthAndState.Email);
        var stageKeyValueList = StageInitializer.CreateInitialKeyValue(itemList, npcList, selectedStageLevel);

        //던전 정보 어디까지 저장할까, 현재 개수와 최대개수도 넣으면 좋다. NPC와 아이템 별개로 할지
        var errorCode = await _memoryDatabase.InsertStageDataAsync(stageKey, stageKeyValueList);
        if (errorCode != ErrorCode.None)
        {
            return errorCode;
        }

        var userKey = MemoryDatabaseKeyGenerator.MakeUIDKey(userAuthAndState.Email);
        errorCode = await _memoryDatabase.UpdateUserStateAsync(userKey, userAuthAndState, UserStateCode.InStage);
        if (errorCode != ErrorCode.None)
        {
            return errorCode;
        }

        return ErrorCode.None;
    }
}