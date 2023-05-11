using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestRespose;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class StartStageController : ControllerBase
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<StartStageController> _logger;

	public StartStageController(ILogger<StartStageController> logger, MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
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

		var errorCode = await _dungeonStageService.CheckStageAccessibility(gameUserId,request.SelectedStageLevel);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.ItemList = _masterDataManager.GetStageItems(request.SelectedStageLevel);
		response.NpcList = _masterDataManager.GetStageNpcs(request.SelectedStageLevel);

		//레디스 상태 저장
		
		response.Error = ErrorCode.None;
		return response;
	}




}