using DungeonWarAPI.ModelDatabase;
using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ViewMailPageController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<ViewMailPageController> _logger;

	public ViewMailPageController(ILogger<ViewMailPageController> logger, IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ViewMailPageResponse> Post(ViewMailPageRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ViewMailPageResponse();

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