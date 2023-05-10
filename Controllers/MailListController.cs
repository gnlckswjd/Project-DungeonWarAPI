using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestRespose;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class MailListController : ControllerBase
{
	private readonly IMailService _mailService;
	private readonly ILogger<MailListController> _logger;

	public MailListController(ILogger<MailListController> logger, IMailService mailService)
	{
		_mailService = mailService;
		_logger = logger;
	}

	[HttpPost]
	public async Task<MailListResponse> Post(MailListRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new MailListResponse();


		var (errorCode, mails) = await _mailService.LoadMailListAsync(authUserData.GameUserId, request.PageNumber);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}


		response.MailsWithItems = mails;
		return response;
	}
}