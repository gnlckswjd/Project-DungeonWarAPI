using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class MailListController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<MailListController> _logger;

	public MailListController(ILogger<MailListController> logger, IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ViewMailPageResponse> Post(ViewMailPageRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ViewMailPageResponse();


		var (errorCode, mails) = await _gameDatabase.LoadMailList(authUserData.GameUserId, request.PageNumber);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}


		response.Mails = mails;
		return response;
	}
}