using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DTO.RequestRespose;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	private readonly IAccountService _accountDatabase;
	private readonly IUserService _userService;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoginController> _logger;

	public LoginController(ILogger<LoginController> logger, IAccountService accountDb, IUserService userService, IMemoryDatabase memoryDb)
	{
		_accountDatabase = accountDb;
		_userService = userService;
		_memoryDatabase = memoryDb;
		_logger = logger;
	}

	[HttpPost]
	public async Task<LoginResponse> Post(LoginRequest request)
	{
		var response = new LoginResponse();
		
		var (errorCode, playerId) = await _accountDatabase.VerifyAccount(request.Email, request.Password);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		(errorCode, var userData) = await _userService.LoadUserDataAsync(playerId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}
		response.UserLevel=userData.UserLevel;


		(errorCode, var items) = await _userService.LoadUserItemsAsync(userData.GameUserId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.items = items;

		var authToken = Security.GetNewToken();

		errorCode = await _memoryDatabase.RegisterUserAsync(request.Email, authToken, userData);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		(errorCode, var notifications) = await _memoryDatabase.LoadNotificationsAsync();

		
		_logger.ZLogInformationWithPayload(new { Email = request.Email, AuthToken = authToken, AccountId =playerId},"Login Success");

		response.Error = errorCode;
		response.Notifications = notifications;
		response.AuthToken = authToken;
		return response;
	}
}