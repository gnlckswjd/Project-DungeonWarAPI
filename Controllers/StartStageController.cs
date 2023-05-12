using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Utilities;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class StartStageController : ControllerBase
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<StartStageController> _logger;

	public StartStageController(ILogger<StartStageController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<StartStageResponse> Post(StartStageRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new StartStageResponse();
		var gameUserId = authUserData.GameUserId;

		var errorCode = await _dungeonStageService.CheckStageAccessibility(gameUserId, request.SelectedStageLevel);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var itemList = _masterDataManager.GetStageItems(request.SelectedStageLevel);
		var npcList = _masterDataManager.GetStageNpcs(request.SelectedStageLevel);

		errorCode = await _memoryDatabase.InitializeStageDataAsync(MemoryDatabaseKeyUtility.MakeStageKey(request.Email),
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