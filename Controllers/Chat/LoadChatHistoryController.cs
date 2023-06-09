﻿using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Chat;
using ZLogger;

namespace DungeonWarAPI.Controllers.Chat;

[Route("[controller]")]
[ApiController]
public class LoadChatHistoryController : Controller
{
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoadChatHistoryController> _logger;

	public LoadChatHistoryController(ILogger<LoadChatHistoryController> logger, IMemoryDatabase memoryDatabase)
	{
		_memoryDatabase = memoryDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<LoadChatHistoryResponse> Post(LoadChatHistoryRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new LoadChatHistoryResponse();

		if (authenticatedUserState == null)
		{
			response.Error = ErrorCode.WrongAuthenticatedUserState;
			return response;
		}

		var key = MemoryDatabaseKeyGenerator.MakeChannelKey(authenticatedUserState.ChannelNumber);

		var (errorCode, chatHistory) = await _memoryDatabase.LoadLatestChatHistoryAsync(key, request.MessageId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { GameUserId = authenticatedUserState.GameUserId, Channel = authenticatedUserState.ChannelNumber,MessageId = request.MessageId },
			"LoadChatHistory Success");

		response.ChatHistory = chatHistory;
		response.Error = ErrorCode.None;
		return response;
	}
}