using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class InAppController : ControllerBase
{
	private readonly IInAppPurchaseDataCRUD _inAppPurchaseDataCRUD;
	private readonly IMailDataCRUD _mailDataCRUD;
	private readonly MasterDataProvider _masterDataProvider;
	private readonly ILogger<InAppController> _logger;

	public InAppController(ILogger<InAppController> logger, MasterDataProvider masterDataProvider, IInAppPurchaseDataCRUD inAppPurchaseDataCRUD, IMailDataCRUD mailDataCrud)
	{
		_inAppPurchaseDataCRUD = inAppPurchaseDataCRUD;
		_mailDataCRUD = mailDataCrud;
		_masterDataProvider = masterDataProvider;
		_logger = logger;
	}

	[HttpPost]
	public async Task<InAppResponse> Post(InAppRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new InAppResponse();

		var gameUserId = userAuthAndState.GameUserId;


		var (errorCode, receiptId) = await _inAppPurchaseDataCRUD.InsertReceiptAsync(gameUserId, request.ReceiptSerialCode, request.PackageId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var packageItems = _masterDataProvider.GetPackageItems(request.PackageId);

		errorCode = await _mailDataCRUD.CreateInAppMailAsync(gameUserId,packageItems);
		if (errorCode != ErrorCode.None)
		{
			await _inAppPurchaseDataCRUD.RollbackStoreReceiptAsync(receiptId);
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { GameUserId=gameUserId, Receipt = request.ReceiptSerialCode, PackageId=request.PackageId},
			"InApp Success");

		response.Error = ErrorCode.None;
		return response;
	}

}