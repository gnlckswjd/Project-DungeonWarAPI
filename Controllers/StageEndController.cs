using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class StageEndController : ControllerBase
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly IItemService _itemService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<StageEndController> _logger;

	public StageEndController(ILogger<StageEndController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService, IItemService itemService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
		_itemService = itemService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<StageEndResponse> Post(StageEndRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new StageEndResponse();
		var gameUserId = authUserData.GameUserId;
		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

		var (errorCode, dictionary) = await _memoryDatabase.LoadStageDataAsync(key);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var (stageLevel, itemCodeAndCount, npcCodeAndCount) = StageDataParser.ParseStageData(dictionary);

		var stageNpcList = _masterDataManager.GetStageNpcList(stageLevel);
		var (isCleared, earnedExp) = StageClearEvaluator.CheckClearAndCalcExp(stageNpcList, npcCodeAndCount);

		if (isCleared == false)
		{
			//await _memoryDatabase.DeleteStageDataAsync(key);
			response.IsCleared = isCleared;
			response.Error = ErrorCode.None;
			return response;
		}

		errorCode = await ProcessStageCompletionAsync(gameUserId, stageLevel, earnedExp, itemCodeAndCount);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		await _memoryDatabase.DeleteStageDataAsync(key);
		response.IsCleared = isCleared;
		response.Error = ErrorCode.None;
		return response;
	}

	private async Task<ErrorCode> ProcessStageCompletionAsync(Int32 gameUserId, Int32 stageLevel, Int32 earnedExp, List<(Int32, Int32)> itemCodeAndCount)
	{
		var(errorCode, isIncrement) = await _dungeonStageService.IncreaseMaxClearedStageAsync(gameUserId, stageLevel);
		if (errorCode != ErrorCode.None)
		{
			return errorCode;
		}

		(errorCode, Int32 existingLevel, Int32 existingExp) = await _dungeonStageService.UpdateExpAsync(gameUserId, earnedExp);
		if (errorCode != ErrorCode.None)
		{
			await _dungeonStageService.RollbackIncreaseMaxClearedStageAsync(gameUserId, isIncrement);

			return errorCode;
		}

		//우편함에 주는 것 고려 정책
		errorCode = await _itemService.InsertItemsAsync(gameUserId, itemCodeAndCount);
		if (errorCode != ErrorCode.None)
		{
			await _dungeonStageService.RollbackUpdateExpAsync(gameUserId, existingLevel, existingExp);
			await _dungeonStageService.RollbackIncreaseMaxClearedStageAsync(gameUserId, isIncrement);

			return errorCode;
		}

		return ErrorCode.None;
	}
}