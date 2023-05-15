using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using ZLogger;

namespace DungeonWarAPI.Controllers;

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
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new StageStartResponse();
		var gameUserId = authUserData.GameUserId;
		var selectedStageLevel = request.SelectedStageLevel;

		var errorCode = await CheckStageAccessibility(gameUserId, selectedStageLevel);
		if (errorCode != 0)
		{
			response.Error = errorCode;
			return response;
		}

		var itemList = _masterDataManager.GetStageItemList(request.SelectedStageLevel);
		var npcList = _masterDataManager.GetStageNpcList(request.SelectedStageLevel);
		if (!itemList.Any() || !npcList.Any())
		{
			response.Error = ErrorCode.WrongStageLevel;
			return response;
		}

		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);
		var stageKeyValueList = StageInitializer.CreateInitialKeyValue(itemList, npcList, selectedStageLevel);

		errorCode = await _memoryDatabase.StoreStageDataAsync(key,stageKeyValueList);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.ItemList = itemList;
		response.NpcList= npcList.ToList();
		response.Error = ErrorCode.None;
		return response;
	}

	private async Task<ErrorCode> CheckStageAccessibility(Int32 gameUserId, Int32 selectedStageLevel)
	{
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
}