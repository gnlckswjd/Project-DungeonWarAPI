using DungeonWarAPI.Models.DTO;
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
		
		//유저 정보 확인
		var (errorCode, playerId) = await _accountDatabase.VerifyAccount(request.Email, request.Password);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		//기본 데이터 게임디비에서 가져오기
		(errorCode, var userData) = await _userService.LoadUserDataAsync(playerId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}
		response.UserLevel=userData.UserLevel;

		//유저 아이템 게임디비에서 가져오기

		(errorCode, var items) = await _userService.LoadUserItemsAsync(userData.GameUserId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.items = items;

		//토큰 발행 후 추가
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