using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Account;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class CreateAccountController : ControllerBase
{
	private readonly IAccountService _accountDatabase;
	private readonly ILogger<CreateAccountController> _logger;
	private readonly OwnedItemFactory _ownedItemFactory;
	private readonly IUserService _userService;
	private readonly  IItemService _itemService;

	public CreateAccountController(IAccountService accountDatabase, IUserService userService, OwnedItemFactory ownedItemFactory,
		ILogger<CreateAccountController> logger, IItemService itemService)
	{
		_accountDatabase = accountDatabase;
		_userService = userService;
		_ownedItemFactory = ownedItemFactory;
		_logger = logger;
		_itemService = itemService;
	}

	[HttpPost]
	public async Task<CreateAccountResponse> Post(CreateAccountRequest request)
	{
		var response = new CreateAccountResponse();

		var (errorCode, accountId )= await _accountDatabase.CreateAccountAsync(request.Email, request.Password);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		errorCode = await CreateDefaultsUserData(accountId);
		if (errorCode != ErrorCode.None)
		{
			response.Error=errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new{Email = request.Email},"CreateAccount Success");
		return response;
	}

	private async Task<ErrorCode> CreateDefaultsUserData(Int32 playerId)
	{
		var (errorCode, gameUserId) = await _userService.CreateUserAsync(playerId);
		if (errorCode != ErrorCode.None)
		{
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		errorCode = await _userService.CreateUserAttendanceAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			await _userService.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		errorCode = await _userService.CreateUserStageAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			await _userService.RollbackCreateUserAttendanceAsync(gameUserId);
			await _userService.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}
		var items = _ownedItemFactory.CreateDefaultItems(gameUserId);

		errorCode = await _itemService.InsertNonStackableItemsAsync(gameUserId, items);
		if (errorCode != ErrorCode.None)
		{
			await _userService.RollbackCreateUserStageAsync(gameUserId);
			await _userService.RollbackCreateUserAttendanceAsync(gameUserId);
			await _userService.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		return ErrorCode.None;
	}
}