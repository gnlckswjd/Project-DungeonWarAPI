using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ItemAcquisitionController : ControllerBase
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<ItemAcquisitionController> _logger;

	public ItemAcquisitionController(ILogger<ItemAcquisitionController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ItemAcquisitionResponse> Post(ItemAcquisitionRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ItemAcquisitionResponse();
		var gameUserId = authUserData.GameUserId;

		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

		var errorCode= await _memoryDatabase.IncrementItemCountAsync(key, request.ItemCode, request.ItemCount);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		
		response.Error = ErrorCode.None;
		return response;
	}

}