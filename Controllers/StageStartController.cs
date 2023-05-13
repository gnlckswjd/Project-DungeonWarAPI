using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;

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

		var errorCode = await _dungeonStageService.CheckStageAccessibilityAsync(gameUserId, request.SelectedStageLevel);
		if (errorCode != ErrorCode.None)
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

		errorCode = await _memoryDatabase.InitializeStageDataAsync(MemoryDatabaseKeyGenerator.MakeStageKey(request.Email),
			itemList, npcList, request.SelectedStageLevel);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.ItemList = itemList;
		response.NpcList= npcList;
		response.Error = ErrorCode.None;
		return response;
	}
}