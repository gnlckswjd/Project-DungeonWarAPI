using DungeonWarAPI.Controllers.Stage;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Chat;
using DungeonWarAPI.Enum;
using ZLogger;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class LoadLatestChatMessageController : ControllerBase
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoadLatestChatMessageController> _logger;

	public LoadLatestChatMessageController(ILogger<LoadLatestChatMessageController> logger, IMemoryDatabase memoryDatabase)
	{
		_memoryDatabase = memoryDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<LoadLatestChatMessageResponse> Post(LoadLatestChatMessageRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new LoadLatestChatMessageResponse();

		if (authenticatedUserState == null)
		{
			response.Error = ErrorCode.WrongAuthenticatedUserState;
			return response;
		}

		var key = MemoryDatabaseKeyGenerator.MakeChannelKey(authenticatedUserState.ChannelNumber);


		var (errorCode, chatMessageReceived )= await _memoryDatabase.LoadLatestChatMessageAsync(key,request.MessageId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { GameUserId = authenticatedUserState.GameUserId, Channel = authenticatedUserState.ChannelNumber, MessageId = request.MessageId }, "LoadLatestChatMessage Success");

		response.Error = ErrorCode.None;
		response.ChatMessage=chatMessageReceived;
		return response;

	}
}