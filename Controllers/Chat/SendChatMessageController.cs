using DungeonWarAPI.Controllers.Stage;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.Enum;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Models.DTO.RequestResponse.Chat;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using DungeonWarAPI.Models.DAO.Redis;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class SendChatMessageController : ControllerBase
{
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<SendChatMessageController> _logger;

	public SendChatMessageController(ILogger<SendChatMessageController> logger, IMemoryDatabase memoryDatabase, MasterDataManager masterDataManager)
	{
		_memoryDatabase = memoryDatabase;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<SendChatResponse> Post(SendChatRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new SendChatResponse();
		
		var key = MemoryDatabaseKeyGenerator.MakeChannelKey(userAuthAndState.ChannelNumber);
		ChatMessageSent chatMessageSent = new ChatMessageSent { Email = userAuthAndState.Email ,Message = request.Message };

		var errorCode = await _memoryDatabase.InsertChatMessageAsync(key, chatMessageSent);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}