using DungeonWarAPI.Controllers.Stage;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Models.DAO.Redis;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class LoadChatMessageHistory : Controller
{
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoadChatMessageHistory> _logger;

	public LoadChatMessageHistory(ILogger<LoadChatMessageHistory> logger, IMemoryDatabase memoryDatabase, MasterDataManager masterDataManager)
	{
		_memoryDatabase = memoryDatabase;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	//[HttpPost]
	//public async Task<Response> Post(Request request)
	//{
	//	var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
	//	var response = new NpcKillResponse();
	//	var gameUserId = userAuthAndState.GameUserId;

	
	//}
}