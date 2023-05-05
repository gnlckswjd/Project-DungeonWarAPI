using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class InAppController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<InAppController> _logger;

	public InAppController(ILogger<InAppController> logger, MasterDataManager masterDataManager,
		IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<InAppResponse> Post(InAppRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new InAppResponse();

		var gameUserId = authUserData.GameUserId;


		var (errorCode, receiptId) = await _gameDatabase.StoreReceiptAsync(gameUserId, request.ReceiptSerialCode, request.PackageId);

		if (errorCode != ErrorCode.None)
		{
			response.Result = errorCode;
			return response;
		}

		var packageItems = _masterDataManager.GetPackageItems(request.PackageId);


		errorCode = await _gameDatabase.CreateInAppMailAsync(gameUserId,packageItems);

		if (errorCode != ErrorCode.None)
		{
			await _gameDatabase.RollbackStoreReceiptAsync(receiptId);
			response.Result = errorCode;
			return response;
		}

		response.Result = ErrorCode.None;
		return response;
	}

}