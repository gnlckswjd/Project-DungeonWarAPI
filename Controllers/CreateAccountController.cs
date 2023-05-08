using DungeonWarAPI;
using DungeonWarAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using DungeonWarAPI.Services.Interfaces;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class CreateAccountController : ControllerBase
{
	private readonly IAccountDatabase _accountDatabase;
	private readonly ILogger<CreateAccountController> _logger;
	private readonly IUserService _userService;

	public CreateAccountController(IAccountDatabase accountDatabase, IUserService userService,
		ILogger<CreateAccountController> logger)
	{
		_accountDatabase = accountDatabase;
		_userService = userService;
		_logger = logger;
	}

	[HttpPost]
	public async Task<CreateAccountResponse> Post(CreateAccountRequest request)
	{
		var response = new CreateAccountResponse();

		var (errorCode, accountId )= await _accountDatabase.CreateAccountAsync(request.Email, request.Password);
		response.Error = errorCode;
		if (errorCode != ErrorCode.None)
		{
			return response;
		}

		(errorCode, var gameUserId) = await _userService.CreateUserAsync(accountId);
		response.Error = errorCode;

		if (errorCode != ErrorCode.None)
		{
			await _accountDatabase.RollbackAccountAsync(accountId);
			return response;
		}

		errorCode = await _userService.CreateUserItemAsync(gameUserId);
		response.Error = errorCode;

		if (errorCode != ErrorCode.None)
		{
			await _userService.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackAccountAsync(accountId);

			return response;
		}

		_logger.ZLogInformationWithPayload(new{Email = request.Email},"CreateAccount Success");
		return response;
	}
}