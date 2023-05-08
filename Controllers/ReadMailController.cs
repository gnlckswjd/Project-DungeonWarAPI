﻿using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services.Interfaces;
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
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ReadMailResponse();

		var ownerId = authUserData.GameUserId;

		var errorCode = await _mailService.MarkMailAsReadAsync(ownerId, request.MailId);

		response.Error = errorCode;
		return response;
	}
}