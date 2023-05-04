using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReadMailController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<ReadMailController> _logger;

	public ReadMailController(ILogger<ReadMailController> logger, IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ReadMailResponse> Post(ReadMailRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ReadMailResponse();

		var ownerId = authUserData.GameUserId;

		var errorCode = await _gameDatabase.MarkMailAsRead(ownerId, request.MailId);

		response.Result = errorCode;
		return response;
	}
}