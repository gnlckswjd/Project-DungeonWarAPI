using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class StageEndController : ControllerBase
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<StageEndController> _logger;

	public StageEndController(ILogger<StageEndController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
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

		var (errorCode, dictionary )= await _memoryDatabase.LoadStageDataAsync(key);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var (stageLevel, itemCodeAndCount, npcCodeAndCount) = MemoryDatabaseKeyGenerator.ParseStageData(dictionary);

		var (isCleared, earnedExp) = _masterDataManager.CheckClearAndGetExp(stageLevel, npcCodeAndCount);

		if (isCleared == false)
		{
			response.IsCleared = isCleared;
			response.Error = ErrorCode.None;
			return response;
		}

		errorCode = await _dungeonStageService.UpdateExpAsync(gameUserId, earnedExp);

		errorCode = await _dungeonStageService.ReceiveRewardItemAsync(gameUserId, itemCodeAndCount);
		if (errorCode != ErrorCode.None)
		{
			response.Error=errorCode;
			return response;
		}


		response.IsCleared = isCleared;
		response.Error = ErrorCode.None;
		return response;
	}
}
