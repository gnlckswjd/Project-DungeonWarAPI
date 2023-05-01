using DungeonWarAPI.ModelDatabase;
using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class MailPageController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<MailPageController> _logger;

	public MailPageController(ILogger<MailPageController> logger, IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<MailPageResponse> Post(MailPageRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new MailPageResponse();

		var (errorCode, mails) = await _gameDatabase.LoadUserMails(authUserData.GameUserId, request.PageNumber);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		//errorCode = await _memoryDatabase.StoreUserMailPageAsync(authUserData, request.PageNumber);

		//if (errorCode != ErrorCode.None)
		//{
		//	response.Result = errorCode;
		//	return response; 
		//}

		response.Mails = mails;
		return response;
	}
}