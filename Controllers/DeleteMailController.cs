using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class DeleteMailController : Controller
{
	private readonly IGameDatabase _gameDatabase;
	private readonly ILogger<DeleteMailController> _logger;

	public DeleteMailController(ILogger<DeleteMailController> logger, IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<DeleteMailResponse> Post(DeleteMailRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new DeleteMailResponse();

		var ownerId = authUserData.GameUserId;

		var errorCode = await _gameDatabase.DeleteMailAsync(ownerId, request.MailId);

		response.Error = errorCode;
		return response;

	}

}