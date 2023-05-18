using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class DeleteMailController : Controller
{
	private readonly IMailService _mailService;
	private readonly ILogger<DeleteMailController> _logger;

	public DeleteMailController(ILogger<DeleteMailController> logger, IMailService mailService)
	{
		_mailService = mailService;
		_logger = logger;
	}

	[HttpPost]
	public async Task<DeleteMailResponse> Post(DeleteMailRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
		var response = new DeleteMailResponse();

		var ownerId = userAuthAndState.GameUserId;

		var errorCode = await _mailService.DeleteMailAsync(ownerId, request.MailId);

		response.Error = errorCode;
		return response;

	}

}