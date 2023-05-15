using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class EnhancementController : ControllerBase
{
	private readonly IEnhancementService _enhancementService;
	private readonly IItemService _itemService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<EnhancementController> _logger;

	public EnhancementController(ILogger<EnhancementController> logger, MasterDataManager masterDataManager,
		IEnhancementService enhancementService, IItemService itemService)
	{
		_enhancementService = enhancementService;
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

		var (errorCode, item) = await _itemService.LoadItemAsync(gameUserId, itemId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var itemCode = item.ItemCode;
		var enhancementCount = item.EnhancementCount;

		var (maxCount, cost) = _masterDataManager.GetEnhanceMaxCountWithCost(itemCode);
		var attributeCode = _masterDataManager.GetAttributeCode(itemCode);

		errorCode = ItemEnhancer.CheckEnhancementPossibility(maxCount, enhancementCount, attributeCode);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		errorCode = await _enhancementService.ValidateEnoughGoldAsync(gameUserId, cost);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}


		Boolean isSuccess = ItemEnhancer.TryEnhancement();

		errorCode = await _enhancementService.UpdateGoldAsync(gameUserId, cost);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		if (isSuccess)
		{
			Int32 itemAttack = ItemEnhancer.GetAttackPower(item.Attack);
			Int32 itemDefense = ItemEnhancer.GetDefensePower(item.Defense);

			errorCode = await _enhancementService.UpdateEnhancementResultAsync(gameUserId, itemId, enhancementCount,
				attributeCode, itemAttack, itemDefense);
			if (errorCode != ErrorCode.None)
			{
				await _enhancementService.RollbackUpdateMoneyAsync(gameUserId, cost);
				response.Error = errorCode;
				return response;
			}
		}
		else
		{
			errorCode = await _itemService.DestroyItemAsync(gameUserId, itemId);
			if (errorCode != ErrorCode.None)
			{
				await _enhancementService.RollbackUpdateMoneyAsync(gameUserId, cost);
				response.Error = errorCode;
				return response;
			}
		}


		errorCode = await _enhancementService.InsertEnhancementHistoryAsync(gameUserId, request.ItemId,
			enhancementCount, isSuccess);
		if (errorCode != ErrorCode.None)
		{
			if (isSuccess)
			{
				await _enhancementService.RollbackUpdateEnhancementCountAsync(itemId,attributeCode,item.Attack,item.Defense, enhancementCount);
			}
			else
			{
				await _itemService.RollbackDestroyItemAsync(itemId);
			}

			await _enhancementService.RollbackUpdateMoneyAsync(gameUserId, cost);
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		response.EnhancementResult = isSuccess;
		return response;
	}
}