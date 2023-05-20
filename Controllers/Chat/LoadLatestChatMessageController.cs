using DungeonWarAPI.Controllers.Stage;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Chat;
using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class LoadLatestChatMessageController : ControllerBase
{
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoadLatestChatMessageController> _logger;

	public LoadLatestChatMessageController(ILogger<LoadLatestChatMessageController> logger, IMemoryDatabase memoryDatabase, MasterDataManager masterDataManager)
	{
		_memoryDatabase = memoryDatabase;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<LoadLatestChatMessageResponse> Post(LoadLatestChatMessageRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new LoadLatestChatMessageResponse();

		var key = MemoryDatabaseKeyGenerator.MakeChannelKey(userAuthAndState.ChannelNumber);


		var (errorCode, chatMessageReceived )= await _memoryDatabase.LoadLatestChatMessageAsync(key,request.MessageId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		response.ChatMessage=chatMessageReceived;
		return response;

	}
}