using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReceiveMailItemController : ControllerBase
{
	private readonly IMailService _mailService;
	private readonly IItemService _itemService;
	private readonly ILogger<ReceiveMailItemController> _logger;

	public ReceiveMailItemController(ILogger<ReceiveMailItemController> logger, IMailService mailService, IItemService itemService)
	{
		_mailService = mailService;
		_itemService = itemService;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ReceiveMailItemResponse> Post(ReceiveMailItemRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ReceiveMailItemResponse();
		var gameUserId = authUserData.GameUserId;

		var errorCode = await _mailService.MarkMailAsReceiveAsync(gameUserId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		(errorCode, var items) = await _mailService.LoadMailItemsAsync(gameUserId, request.MailId);
		if (errorCode != ErrorCode.None)
		{
			await _mailService.RollbackMarkMailItemAsReceiveAsync(gameUserId, request.MailId);
			response.Error = errorCode;
			return response;
		}

		errorCode = await _itemService.InsertItemsAsync(gameUserId, items);
		if (errorCode != ErrorCode.None)
		{
			await _mailService.RollbackMarkMailItemAsReceiveAsync(gameUserId, request.MailId);
			response.Error=errorCode;
			return response;
		}


		response.Error = errorCode;
		return response;
	}
}