using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class InAppController : ControllerBase
{
	private readonly IInAppPurchaseService _inAppPurchaseService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<InAppController> _logger;

	public InAppController(ILogger<InAppController> logger, MasterDataManager masterDataManager,
		IInAppPurchaseService inAppPurchaseService)
	{
		_inAppPurchaseService = inAppPurchaseService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<InAppResponse> Post(InAppRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
		var response = new InAppResponse();

		var gameUserId = userAuthAndState.GameUserId;


		var (errorCode, receiptId) = await _inAppPurchaseService.StoreReceiptAsync(gameUserId, request.ReceiptSerialCode, request.PackageId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var packageItems = _masterDataManager.GetPackageItems(request.PackageId);


		errorCode = await _inAppPurchaseService.CreateInAppMailAsync(gameUserId,packageItems);

		if (errorCode != ErrorCode.None)
		{
			await _inAppPurchaseService.RollbackStoreReceiptAsync(receiptId);
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}

}