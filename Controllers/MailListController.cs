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
	public async Task<MailListResponse> Post(MailListRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new MailListResponse();


		var (errorCode, mails) = await _gameDatabase.LoadMailListAsync(authUserData.GameUserId, request.PageNumber);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}


		response.MailsWithItems = mails;
		return response;
	}
}