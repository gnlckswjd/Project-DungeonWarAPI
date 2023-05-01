using DungeonWarAPI.ModelDatabase;
using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReceiveMailItemController : Controller
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<ReceiveMailItemController> _logger;

	public ReceiveMailItemController(ILogger<ReceiveMailItemController> logger, IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ReceiveMailItemResponse> Post(ReceiveMailItemRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ReceiveMailItemResponse();
		var ownerId = authUserData.GameUserId;

		var errorCode = await _gameDatabase.VerifyMailOwnerId(ownerId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		(errorCode, var mail) = await _gameDatabase.MarkMailItemAsReceive(ownerId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		errorCode = await _gameDatabase.ReceiveItemAsync(ownerId, mail);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			await _gameDatabase.RollbackMarkMailItemAsReceiveAsync(ownerId, request.MailId);
			return response;
		}



		response.Result = errorCode;
		return response;
	}
}