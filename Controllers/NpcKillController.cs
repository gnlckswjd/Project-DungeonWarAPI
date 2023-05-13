using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

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
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new NpcKillResponse();
		var gameUserId = authUserData.GameUserId;

		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

		// HMGET 에서 존재하지 않는 필드로 Get 요청을 하면 Nil
		
		var (errorCode, stageLevel) = await _memoryDatabase.LoadStageLevelAsync(key);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		//Todo : stageLevel을 레디스에서 읽어온 뒤 존재하는 코드인지 확인하고 개수가 넘는지 확인하는 로직 추가
		

		var stageNpc =  _masterDataManager.GetNpcByStageAndCode(stageLevel, request.NpcCode);
		if (stageNpc == null)
		{
			response.Error = ErrorCode.WrongNpcCode;
			return response;
		}

		//Todo : redis에서 개수 읽어오기
		(errorCode, var npcKillCount) = await _memoryDatabase.LoadNpcKillCountAsync(key, request.NpcCode);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}
		//ToDo : Killcount 비교하고 리턴


		errorCode = await _memoryDatabase.IncrementNpcKillCountAsync(key, request.NpcCode);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}