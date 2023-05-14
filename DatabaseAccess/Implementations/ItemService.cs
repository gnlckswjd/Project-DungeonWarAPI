using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.Database.Game;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class ItemService : IItemService
{
	private readonly ILogger<ItemService> _logger;
	private readonly OwnedItemFactory _ownedItemFactory;
	private readonly QueryFactory _queryFactory;

	public ItemService(ILogger<ItemService> logger, QueryFactory queryFactory, OwnedItemFactory ownedItemFactory)
	{
		_logger = logger;
		_ownedItemFactory = ownedItemFactory;
		_queryFactory = queryFactory;
	}

	public async Task<ErrorCode> InsertItemsAsync(Int32 gameUserId, List<MailItem> items)
	{
		List<Func<Task>> rollbackActions = new List<Func<Task>>();
		try
		{
			foreach (var item in items)
			{
				ErrorCode errorCode;

				if (item.ItemCode == 1)
				{
					errorCode = await IncreaseGoldAsync(gameUserId, item.ItemCount, rollbackActions);
				}
				else if (item.ItemCode == 6)
				{
					errorCode = await IncreasePotionAsync(gameUserId, item.ItemCount, rollbackActions);
				}
				else
				{
					errorCode = await InsertOwnedItemAsync(gameUserId, item.ItemCode, item.ItemCount,
						item.EnhancementCount, rollbackActions);
				}

				if (errorCode != ErrorCode.None)
				{
					await RollbackReceiveItemAsync(rollbackActions);

					_logger.ZLogErrorWithPayload(
						new
						{
							ErrorCode = ErrorCode.ReceiveItemFailInsert,
							GameUserId = gameUserId
						},
						"ReceiveItemFailInsert");

					return ErrorCode.ReceiveItemFailInsert;
				}
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			await RollbackReceiveItemAsync(rollbackActions);
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.ReceiveItemFailException,
					GameUserId = gameUserId
				},
				"ReceiveItemFailException");
			return ErrorCode.ReceiveItemFailException;
		}
	}


	private async Task<ErrorCode> IncreaseGoldAsync(Int32 gameUserId, Int32 itemCount, List<Func<Task>> rollbackActions)
	{
		var count = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
			.IncrementAsync("Gold", itemCount);

		if (count != 1)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.IncreaseGoldFailUpdate, GameUserId = gameUserId },
				"IncreaseGoldFailUpdate");
			return ErrorCode.IncreaseGoldFailUpdate;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.DecrementAsync("Gold", itemCount);
			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackIncreaseGoldFail,
						GameUserId = gameUserId,
						ItemCount = itemCount
					}, "RollbackIncreaseGoldFail");
			}
		});
		return ErrorCode.None;
	}


	private async Task<ErrorCode> IncreasePotionAsync(Int32 gameUserId, Int32 itemCount,
		List<Func<Task>> rollbackActions)
	{
		ErrorCode errorCode = ErrorCode.None;
		var count = await _queryFactory.Query("owned_item").Where("GameUserId", "=", gameUserId)
			.Where("ItemCode", "=", (Int32)ItemCode.Potion)
			.IncrementAsync("ItemCount", itemCount);
		if (count == 0)
		{
			errorCode = await InsertOwnedItemAsync(gameUserId, 6, itemCount,
				0, rollbackActions);
		}

		if (errorCode != ErrorCode.None)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.IncreasePotionFailUpdateOrInsert, GameUserId = gameUserId },
				"IncreasePotionFailUpdateOrInsert");
			return ErrorCode.IncreasePotionFailUpdateOrInsert;
		}

		// count == 0 일 때 아이템 생성에 관한 롤백 등록은 InsertOwnedItemAsync에서 진행
		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("owned_item")
				.Where("GameUserId", "=", gameUserId)
				.Where("ItemCode", "=", (Int32)ItemCode.Potion)
				.DecrementAsync("ItemCount", itemCount);

			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackIncreasePotionFail,
						GameUserId = gameUserId,
						ItemCount = itemCount
					}, "RollbackIncreasePotionFail");
			}
		});

		return ErrorCode.None;
	}

	private async Task<ErrorCode> InsertOwnedItemAsync(Int32 gameUserId, Int32 itemCode, Int32 itemCount,
		Int32 enhancementCount, List<Func<Task>> rollbackActions)
	{
		var itemId = await _queryFactory.Query("owned_item")
			.InsertGetIdAsync<int>(
				_ownedItemFactory.CreateOwnedItem(gameUserId, itemCode, enhancementCount, itemCount));

		if (itemId == 0)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.InsertOwnedItemFailInsert, GameUserId = gameUserId },
				"InsertOwnedItemFailInsert");
			return ErrorCode.InsertOwnedItemFailInsert;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("user_data").Where("ItemId", "=", itemId)
				.DeleteAsync();

			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackInsertOwnedItemFail,
						GameUserId = gameUserId,
						ItemId = itemId
					}, "RollbackInsertOwnedItemFail");
			}
		});

		return ErrorCode.None;
	}

	private async Task RollbackReceiveItemAsync(List<Func<Task>> rollbackActions)
	{
		foreach (var action in rollbackActions)
		{
			await action();
		}
	}
}