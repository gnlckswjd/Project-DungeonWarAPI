using DungeonWarAPI.Controllers.Stage;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.Enum;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Models.DTO.RequestResponse.Chat;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.Database.Game;
using ZLogger;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class SendChatMessageController : ControllerBase
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<SendChatMessageController> _logger;

	public SendChatMessageController(ILogger<SendChatMessageController> logger, IMemoryDatabase memoryDatabase)
	{
		_memoryDatabase = memoryDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<SendChatResponse> Post(SendChatRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new SendChatResponse();

		if (authenticatedUserState == null)
		{
			response.Error = ErrorCode.WrongAuthenticatedUserState;
			return response;
		}

		var key = MemoryDatabaseKeyGenerator.MakeChannelKey(authenticatedUserState.ChannelNumber);

		ChatMessageSent chatMessageSent = new ChatMessageSent { Email = authenticatedUserState.Email ,Message = request.Message };

		var errorCode = await _memoryDatabase.InsertChatMessageAsync(key, chatMessageSent);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { GameUserId = authenticatedUserState.GameUserId, Channel = authenticatedUserState.ChannelNumber, }, "SendChatMessage Success");

		response.Error = ErrorCode.None;
		return response;
	}
}