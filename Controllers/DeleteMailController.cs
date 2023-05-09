using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestRespose;
using DungeonWarAPI.Services.Interfaces;
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
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new DeleteMailResponse();

		var ownerId = authUserData.GameUserId;

		var errorCode = await _mailService.DeleteMailAsync(ownerId, request.MailId);

		response.Error = errorCode;
		return response;

	}

}