using DungeonWarAPI.ModelPacket;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	private readonly IAccountDatabase _accountDatabase;
	private readonly IGameDatabase _gameDatabase;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoginController> _logger;

	public LoginController(ILogger<LoginController> logger, IAccountDatabase accountDb, IGameDatabase gameDatabase, IMemoryDatabase memoryDb)
	{
		_accountDatabase = accountDb;
		_gameDatabase = gameDatabase;
		_memoryDatabase = memoryDb;
		_logger = logger;
	}

	[HttpPost]
	public async Task<LoginResponse> Post(LoginRequest request)
	{
		var response = new LoginResponse();

		//유저 정보 확인
		var (errorCode, accountId) = await _accountDatabase.VerifyAccount(request.Email, request.Password);
		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		//토큰 발행 후 추가
		var authToken = Security.GetNewToken();

		errorCode = await _memoryDatabase.RegisterUserAsync(request.Email, authToken, accountId);

		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		var (noticeErrorCode, notifications) = await _memoryDatabase.LoadNoticeAsync();

		_logger.ZLogInformationWithPayload(new { Email = request.Email, AuthToken = authToken, AccountId =accountId},"Login Success");

		response.Notifications = notifications;
		response.AuthToken = authToken;
		return response;
	}
}