using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReadMailController : ControllerBase
{
	private readonly IMailService _mailService;
	private readonly ILogger<ReadMailController> _logger;

	public ReadMailController(ILogger<ReadMailController> logger, IMailService mailService)
	{
		_mailService = mailService;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ReadMailResponse> Post(ReadMailRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
		var response = new ReadMailResponse();

		var ownerId = userAuthAndState.GameUserId;

		var (errorCode, content )= await _mailService.ReadMailAsync(ownerId, request.MailId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = errorCode;
		response.Content = content;
		return response;
	}
}