using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class NpcKillController : Controller
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<NpcKillController> _logger;

	public NpcKillController(ILogger<NpcKillController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<NpcKillResponse> Post(NpcKillRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
		var response = new NpcKillResponse();
		var gameUserId = userAuthAndState.GameUserId;

		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

		var (errorCode, npcKillCount, maxNpcCount) = await LoadKillAndMaxNpcCountAsync(key, request.NpcCode,gameUserId,userAuthAndState.State);
		if (errorCode != ErrorCode.None)
		{
			response.Error=errorCode;
			return response;
		}

		if (npcKillCount >= maxNpcCount)
		{
			response.Error = ErrorCode.ExceedKillCount;
			return response;
		}

		errorCode = await _memoryDatabase.IncrementNpcKillCountAsync(key, request.NpcCode);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}

	private async Task<(ErrorCode, Int32 killCount, Int32 maxNpcCount)> LoadKillAndMaxNpcCountAsync(String key, Int32 npcCode,Int32 gameUserId, UserStateCode state)
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
			return (errorCode, 0,0);
		}

		var stageNpc = _masterDataManager.GetStageNpcByStageAndCode(stageLevel, npcCode);
		if (stageNpc == null)
		{
			return (ErrorCode.WrongNpcCode, 0, 0);
		}

		(errorCode, var npcKillCount) = await _memoryDatabase.LoadNpcKillCountAsync(key, npcCode);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, 0, 0);
		}

		return (ErrorCode.None, npcKillCount, stageNpc.NpcCount);
	}
}