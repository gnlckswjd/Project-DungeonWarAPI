using DungeonWarAPI.ModelDatabase;
using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class MailReadController : Controller
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<MailReadController> _logger;

	public MailReadController(ILogger<MailReadController> logger, IGameDatabase gameDatabase )
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<MailReadResponse> Post(MailReadRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new MailReadResponse();

		var errorCode = await _gameDatabase.MarkMailAsRead(authUserData.GameUserId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		response.Result= errorCode;
		return response;
	}
}