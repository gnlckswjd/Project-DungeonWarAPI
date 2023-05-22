using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	private readonly IAccountDataCRUD _accountDatabase;
	private readonly IUserDataCRUD _userDataCRUD;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<LoginController> _logger;

	public LoginController(ILogger<LoginController> logger, IAccountDataCRUD accountDb, IUserDataCRUD userDataCRUD, IMemoryDatabase memoryDb)
	{
		_accountDatabase = accountDb;
		_userDataCRUD = userDataCRUD;
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

		(errorCode, var userData, var items) = await LoadDatAsync(playerId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var authToken = Security.GetNewAuthToken();

		errorCode = await _memoryDatabase.RegisterUserAsync(request.Email, authToken, userData);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		(errorCode, var notifications) = await _memoryDatabase.LoadNotificationsAsync();
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { Email = request.Email, AuthToken = authToken, AccountId = playerId }, 
			"Login Success");

		response.Error = errorCode;
		response.UserLevel = userData.UserLevel;
		response.Items = items;
		response.Notifications = notifications;
		response.AuthToken = authToken;
		return response;
	}

	private async Task<(ErrorCode, UserData, List<OwnedItem>)> LoadDatAsync(Int32 playerId)
	{
		var (errorCode, userData) = await _userDataCRUD.LoadUserDataAsync(playerId);

		if (errorCode != ErrorCode.None)
		{
			return (errorCode, default, default);
		}

		(errorCode, var items) = await _userDataCRUD.LoadUserItemsAsync(userData.GameUserId);

		if (errorCode != ErrorCode.None)
		{
			return (errorCode, default, default);
		}

		return (ErrorCode.None, userData, items);
	}
}