using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestRespose;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReceiveMailItemController : ControllerBase
{
	private readonly IMailService _mailService;
	private readonly ILogger<ReceiveMailItemController> _logger;

	public ReceiveMailItemController(ILogger<ReceiveMailItemController> logger, IMailService mailService)
	{
		_mailService = mailService;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ReceiveMailItemResponse> Post(ReceiveMailItemRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ReceiveMailItemResponse();
		var ownerId = authUserData.GameUserId;


		var errorCode = await _mailService.MarkMailAsReceiveAsync(ownerId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		errorCode = await _mailService.ReceiveItemAsync(ownerId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			await _mailService.RollbackMarkMailItemAsReceiveAsync(ownerId, request.MailId);
			return response;
		}


		response.Error = errorCode;
		return response;
	}
}