using DungeonWarAPI.Services;
using DungeonWarAPI;
using DungeonWarAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class CreateAccountController : ControllerBase
{
	private readonly IAccountDatabase _accountDatabase;
	private readonly ILogger<CreateAccountController> _logger;
	private readonly IGameDatabase _gameDatabase;

	public CreateAccountController(IAccountDatabase accountDatabase, IGameDatabase gameDatabase,
		ILogger<CreateAccountController> logger)
	{
		_accountDatabase = accountDatabase;
		_gameDatabase = gameDatabase;
		_logger = logger;
	}

	[HttpPost]
	public async Task<CreateAccountResponse> Post(CreateAccountRequest request)
	{
		var response = new CreateAccountResponse();

		var (errorCode, accountId )= await _accountDatabase.CreateAccountAsync(request.Email, request.Password);
		response.Result = errorCode;
		if (errorCode != ErrorCode.None)
		{
			return response;
		}

		(errorCode, var gameUserId) = await _gameDatabase.CreateUserAsync(accountId);
		response.Result = errorCode;

		if (errorCode != ErrorCode.None)
		{
			await _accountDatabase.RollbackAccountAsync(accountId);
			return response;
		}

		errorCode = await _gameDatabase.CreateUserItemAsync(gameUserId);
		response.Result = errorCode;

		if (errorCode != ErrorCode.None)
		{
			await _gameDatabase.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackAccountAsync(accountId);

			return response;
		}

		_logger.ZLogInformationWithPayload(new{Email = request.Email},"CreateAccount Success");
		return response;
	}
}