using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using DungeonWarAPI;
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
		var guid = Security.GetNewGUID();

		var errorCode = await _accountDatabase.CreateAccountAsync(request.Email, request.Password, guid);
		response.Result = errorCode;
		if (errorCode != ErrorCode.None)
		{
			return response;
		}

		var errorCodeGame = await _gameDatabase.CreateUserAsync(guid);
		response.Result = errorCodeGame;

		if (errorCodeGame != ErrorCode.None)
		{
			await _accountDatabase.RollbackAccountAsync(guid);
			return response;
		}

		var errorCodeItem = await _gameDatabase.CreateUserItemAsync(guid);
		response.Result = errorCodeItem;

		if (errorCodeItem != ErrorCode.None)
		{
			await _gameDatabase.RollbackUserAsync(guid);
			await _accountDatabase.RollbackAccountAsync(guid);
		}

		_logger.ZLogInformationWithPayload(new{Email = request.Email},"CreateAccount Success");
		return response;
	}
}