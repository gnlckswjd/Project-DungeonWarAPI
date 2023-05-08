using DungeonWarAPI.Managers;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class EnhancementController : ControllerBase
{
	private readonly IItemService _itemService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<EnhancementController> _logger;

	public EnhancementController(ILogger<EnhancementController> logger, MasterDataManager masterDataManager,
		IItemService itemService)
	{
		_itemService = itemService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<EnhancementResponse> Post(EnhancementRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new EnhancementResponse();

		var gameUserId = authUserData.GameUserId;
		var itemId = request.ItemId;

		var (errorCode, itemCode, enhancementCount) = await _itemService.LoadItemAsync(gameUserId, itemId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var (maxCount, gold) = _masterDataManager.GetEnhanceMaxCountWithGold(itemCode);

		errorCode = ItemEnhancementManager.CheckEnhancementPossibility(maxCount, enhancementCount);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		Boolean isSuccess = ItemEnhancementManager.EnhanceItem();


		errorCode = await _itemService.UpdateGoldAsync(gameUserId, gold);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		if (isSuccess)
		{
			errorCode = await _itemService.UpdateEnhancementCountAsync(gameUserId, itemId, enhancementCount);
			if (errorCode != ErrorCode.None)
			{
				_itemService.RollbackUpdateMoneyAsync(gameUserId, gold);
				response.Error = errorCode;
				return response;
			}
		}
		else
		{
			errorCode = await _itemService.DestroyItemAsync(gameUserId, itemId);
			if (errorCode != ErrorCode.None)
			{
				_itemService.RollbackUpdateMoneyAsync(gameUserId, gold);
				response.Error = errorCode;
				return response;
			}
		}


		errorCode = await _itemService.InsertEnhancementHistoryAsync(gameUserId, request.ItemId, enhancementCount,
			isSuccess);
		if (errorCode != ErrorCode.None)
		{
			if (isSuccess)
			{
				_itemService.RollbackUpdateEnhancementCountAsync(itemId);
			}
			else
			{
				_itemService.RollbackDestroyItemAsync(itemId);
			}

			_itemService.RollbackUpdateMoneyAsync(gameUserId, gold);
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		response.EnhancementResult = isSuccess;
		return response;
	}
}