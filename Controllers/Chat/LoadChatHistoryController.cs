using DungeonWarAPI.Controllers.Stage;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Chat;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class LoadChatHistoryController : Controller
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoadChatHistoryController> _logger;

	public LoadChatHistoryController(ILogger<LoadChatHistoryController> logger, IMemoryDatabase memoryDatabase)
	{
		_memoryDatabase = memoryDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<LoadChatHistoryResponse> Post(LoadChatHistoryRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new LoadChatHistoryResponse();

		var key = MemoryDatabaseKeyGenerator.MakeChannelKey(authenticatedUserState.ChannelNumber);
		var (errorCode, chatHistory) = await _memoryDatabase.LoadLatestChatHistoryAsync(key, request.MessageId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.ChatHistory = chatHistory;
		response.Error = ErrorCode.None;
		return response;
	}
}