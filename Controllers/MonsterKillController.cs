using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using DungeonWarAPI.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class MonsterKillController : Controller
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<MonsterKillController> _logger;

	public MonsterKillController(ILogger<MonsterKillController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<MonsterKillResponse> Post(MonsterKillRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new MonsterKillResponse();
		var gameUserId = authUserData.GameUserId;

		var key = MemoryDatabaseKeyUtility.MakeStageKey(request.Email);

		var errorCode = await _memoryDatabase.IncrementNpcKillCountAsync(key, request.NpcCode);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}