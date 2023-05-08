using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class EnhancementController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<EnhancementController> _logger;

	public EnhancementController(ILogger<EnhancementController> logger, MasterDataManager masterDataManager,
		IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
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

		var (errorCode, itemCode, enhancementCount) = await _gameDatabase.LoadItemAsync(gameUserId, itemId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var (maxCount, gold) = _masterDataManager.GetEnhanceMaxCountWithGold(itemCode);

		errorCode = ItemManager.CheckEnhancementPossibility(maxCount, enhancementCount);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		Boolean isSuccess = ItemManager.EnhanceItem();


		errorCode = await _gameDatabase.UpdateGoldAsync(gameUserId, gold);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		if (isSuccess)
		{
			errorCode = await _gameDatabase.UpdateEnhancementCountAsync(gameUserId, itemId, enhancementCount);
			if (errorCode != ErrorCode.None)
			{
				_gameDatabase.RollbackUpdateMoneyAsync(gameUserId, gold);
				response.Error = errorCode;
				return response;
			}
		}
		else
		{
			errorCode = await _gameDatabase.DestroyItemAsync(gameUserId, itemId);
		}


		errorCode = await _gameDatabase.InsertEnhancementHistoryAsync(gameUserId, request.ItemId, enhancementCount,
			isSuccess);
		if (errorCode != ErrorCode.None)
		{
			if (isSuccess)
			{
				_gameDatabase.RollbackUpdateEnhancementCountAsync(itemId);
			}
			else
			{
				_gameDatabase.RollbackDestroyItem(itemId);
			}

			_gameDatabase.RollbackUpdateMoneyAsync(gameUserId, gold);
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		response.EnhancementResult = isSuccess;
		return response;
	}
}