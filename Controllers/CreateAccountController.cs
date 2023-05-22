using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ZLogger;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class CreateAccountController : ControllerBase
{
	private readonly IAccountDataCRUD _accountDatabase;
	private readonly ILogger<CreateAccountController> _logger;
	private readonly OwnedItemFactory _ownedItemFactory;
	private readonly IUserDataCRUD _userDataCRUD;
	private readonly  IItemDATACRUD _itemDataCRUD;

	public CreateAccountController(IAccountDataCRUD accountDatabase, IUserDataCRUD userDataCRUD, OwnedItemFactory ownedItemFactory,
		ILogger<CreateAccountController> logger, IItemDATACRUD itemDataCRUD)
	{
		_accountDatabase = accountDatabase;
		_userDataCRUD = userDataCRUD;
		_ownedItemFactory = ownedItemFactory;
		_logger = logger;
		_itemDataCRUD = itemDataCRUD;
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
		var (errorCode, gameUserId) = await _userDataCRUD.CreateUserAsync(playerId);
		if (errorCode != ErrorCode.None)
		{
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		errorCode = await _userDataCRUD.CreateUserAttendanceAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			await _userDataCRUD.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		errorCode = await _userDataCRUD.CreateUserStageAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			await _userDataCRUD.RollbackCreateUserAttendanceAsync(gameUserId);
			await _userDataCRUD.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		var items = _ownedItemFactory.CreateDefaultItems(gameUserId);

		errorCode = await _itemDataCRUD.InsertNonStackableItemsAsync(gameUserId, items);
		if (errorCode != ErrorCode.None)
		{
			await _userDataCRUD.RollbackCreateUserStageAsync(gameUserId);
			await _userDataCRUD.RollbackCreateUserAttendanceAsync(gameUserId);
			await _userDataCRUD.RollbackCreateUserAsync(gameUserId);
			await _accountDatabase.RollbackCreateAccountAsync(playerId);
			return errorCode;
		}

		return ErrorCode.None;
	}
}