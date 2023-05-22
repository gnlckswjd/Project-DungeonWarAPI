using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Implementations;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class EnhancementController : ControllerBase
{
	private readonly IEnhancementDataCRUD _enhancementDataCRUD;
	private readonly IItemDATACRUD _itemDataCRUD;
	private readonly IUserDataCRUD _userDataCRUD;
	private readonly MasterDataProvider _masterDataProvider;
	private readonly ILogger<EnhancementController> _logger;

	public EnhancementController(ILogger<EnhancementController> logger, MasterDataProvider masterDataProvider,
		IEnhancementDataCRUD enhancementDataCRUD, IItemDATACRUD itemDataCRUD, IUserDataCRUD userDataCrud)
	{
		_enhancementDataCRUD = enhancementDataCRUD;
		_itemDataCRUD = itemDataCRUD;
		_userDataCRUD = userDataCrud;
		_masterDataProvider = masterDataProvider;
		_logger = logger;
	}

	[HttpPost]
	public async Task<EnhancementResponse> Post(EnhancementRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new EnhancementResponse();

		if (authenticatedUserState == null)
		{
			response.Error = ErrorCode.WrongAuthenticatedUserState;
			return response;
		}

		var gameUserId = authenticatedUserState.GameUserId;
		var itemId = request.ItemId;

		var (errorCode, item, gold) = await LoadItemAndGoldAsync(gameUserId, itemId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var itemCode = item.ItemCode;
		var maxCount = _masterDataProvider.GetEnhanceMaxCountWithCost(itemCode);
		var attributeCode = _masterDataProvider.GetAttributeCode(itemCode);
		var enhancementCount = item.EnhancementCount;

		(errorCode, var cost) = ItemEnhancer.VerifyEnhancementPossibilityAndGetCost(maxCount, enhancementCount, attributeCode, gold);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		(errorCode, var isSuccess) = await ProcessEnhancement(gameUserId, cost, itemId, enhancementCount, attributeCode, item.Attack, item.Defense);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		errorCode = await _enhancementDataCRUD.InsertEnhancementHistoryAsync(gameUserId, itemId, enhancementCount, isSuccess);
		if (errorCode != ErrorCode.None)
		{
			if (isSuccess)
			{
				await _enhancementDataCRUD.UpdateEnhancementResultAsync(gameUserId, itemId, enhancementCount,
					attributeCode, item.Attack, item.Defense);
			}
			else
			{
				await _itemDataCRUD.RollbackDestroyItemAsync(itemId);
			}

			await _userDataCRUD.RollbackUpdateMoneyAsync(gameUserId, cost);
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { GameUserId=gameUserId, ItemId = request.ItemId, IsSuccess=isSuccess}, 
			"Enhancement Success");

		response.Error = ErrorCode.None;
		response.EnhancementResult = isSuccess;
		return response;
	}

	private async Task<(ErrorCode, OwnedItem, Int64 gold)> LoadItemAndGoldAsync(Int32 gameUserId, Int64 itemId)
	{
		var (errorCode, item) = await _itemDataCRUD.LoadItemAsync(gameUserId, itemId);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, default, 0);
		}

		(errorCode, var gold) = await _userDataCRUD.LoadGoldAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, default, 0);
		}

		return (ErrorCode.None, item, gold);
	}

	private async Task<(ErrorCode, Boolean)> ProcessEnhancement(Int32 gameUserId, Int32 cost, Int64 itemId,
		Int32 enhancementCount, Int32 attributeCode, Int32 attack, Int32 defense)
	{
		var errorCode = await _userDataCRUD.UpdateGoldAsync(gameUserId, cost);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, false);
		}

		Boolean isSuccess = ItemEnhancer.TryEnhancement();

		if (isSuccess)
		{
			Int32 itemAttack = ItemEnhancer.GetEnhancedAttackPower(attack);
			Int32 itemDefense = ItemEnhancer.GetEnhancedDefensePower(defense);

			errorCode = await _enhancementDataCRUD.UpdateEnhancementResultAsync(gameUserId, itemId, enhancementCount, attributeCode, itemAttack, itemDefense);
		}
		else
		{
			errorCode = await _itemDataCRUD.UpdateItemStatusToDestroyAsync(gameUserId, itemId);
		}

		if (errorCode != ErrorCode.None)
		{
			await _userDataCRUD.RollbackUpdateMoneyAsync(gameUserId, cost);

			return (errorCode, false);
		}

		return (ErrorCode.None, isSuccess);
	}
}