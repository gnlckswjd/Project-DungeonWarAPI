using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.Database.Game;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class ItemService : DatabaseAccessBase, IItemService
{
	private readonly OwnedItemFactory _ownedItemFactory;


	public ItemService(ILogger<ItemService> logger, QueryFactory queryFactory, OwnedItemFactory ownedItemFactory) 
		:base(logger,queryFactory)
	{
		_ownedItemFactory = ownedItemFactory;

	}
	public async Task<(ErrorCode, OwnedItem)> LoadItemAsync(Int32 gameUserId, Int64 itemId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ItemId = itemId }, "LoadItem Start");
		try
		{
			var item = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.FirstOrDefaultAsync<OwnedItem>();

			if (item == null)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadItemFailSelect, GameUserId = gameUserId, ItemId = itemId },
					"LoadItemFailSelect");
				return (ErrorCode.LoadItemFailSelect, new OwnedItem());
			}


			if (item.GameUserId != gameUserId)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadItemFailWrongGameUser, GameUserId = gameUserId, ItemId = itemId },
					"LoadItemFailWrongGameUser");
				return (ErrorCode.LoadItemFailWrongGameUser, new OwnedItem());
			}


			return (ErrorCode.None, item);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new { ErrorCode = ErrorCode.LoadItemFailException, GameUserId = gameUserId, ItemId = itemId },
				"LoadItemFailException");
			return (ErrorCode.LoadItemFailException, new OwnedItem());
		}
	}

	public async Task<ErrorCode> DestroyItemAsync(Int32 gameUserId, Int64 itemId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ItemId = itemId }, "DestroyItem Start");

		try
		{
			var count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.UpdateAsync(new { IsDestroyed = true });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.DestroyItemFailUpdate, GameUserId = gameUserId, ItemId = itemId },
					"DestroyItemFailUpdate");

				return ErrorCode.DestroyItemFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new { ErrorCode = ErrorCode.DestroyItemFailException, GameUserId = gameUserId, ItemId = itemId },
				"DestroyItemFailException");

			return ErrorCode.DestroyItemFailException;
		}
	}


	public async Task<ErrorCode> InsertItemsAsync(Int32 gameUserId, List<MailItem> items)
	{
		List<Func<Task>> rollbackActions = new List<Func<Task>>();
		try
		{
			foreach (var item in items)
			{
				ErrorCode errorCode= await AddItemBasedOnCodeAsync(gameUserId,item.ItemCode,item.ItemCount,rollbackActions);


				if (errorCode != ErrorCode.None)
				{
					await RollbackReceiveItemAsync(rollbackActions);

					_logger.ZLogErrorWithPayload(
						new
						{
							ErrorCode = ErrorCode.InsertItemFailInsert,
							GameUserId = gameUserId
						},
						"InsertItemFailInsert");

					return ErrorCode.InsertItemFailInsert;
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
					ErrorCode = ErrorCode.InsertItemFailException,
					GameUserId = gameUserId
				},
				"InsertItemFailException");
			return ErrorCode.InsertItemFailException;
		}
	}

	public async Task<ErrorCode> InsertItemsAsync(Int32 gameUserId, List<(Int32, Int32)> itemCodeList)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "ReceiveRewardItem Start");
		List<Func<Task>> rollbackActions = new List<Func<Task>>();
		try
		{
			foreach (var (itemCode, itemCount) in itemCodeList)
			{
				ErrorCode errorCode = await AddItemBasedOnCodeAsync(gameUserId,itemCode,itemCount,rollbackActions);

				if (errorCode != ErrorCode.None)
				{
					await RollbackReceiveItemAsync(rollbackActions);

					_logger.ZLogErrorWithPayload(
						new
						{
							ErrorCode = ErrorCode.InsertItemFailInsert,
							GameUserId = gameUserId
						},
						"InsertItemFailInsert");

					return ErrorCode.InsertItemFailInsert;
				}
			}


			return ErrorCode.None;
		}
		catch (Exception e)
		{
			await RollbackReceiveItemAsync(rollbackActions);
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.InsertItemFailException, GameUserId = gameUserId }
				, "InsertItemFailException");
			return ErrorCode.InsertItemFailException;
		}
	}

	private async Task<ErrorCode> AddItemBasedOnCodeAsync(Int32 gameUserId, Int32 itemCode ,Int32 itemCount, List<Func<Task>> rollbackActions)
	{
		ErrorCode errorCode= ErrorCode.None;
		if (itemCode == (int)ItemCode.Gold)
		{
			errorCode = await IncreaseGoldAsync(gameUserId, itemCount, rollbackActions);
		}
		else if (itemCode == (int)ItemCode.Potion)
		{
			errorCode = await IncreasePotionAsync(gameUserId, itemCount, rollbackActions);
		}
		else
		{
			errorCode = await InsertOwnedItemAsync(gameUserId, itemCode, itemCount,
				0, rollbackActions);
		}

		return errorCode;
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
	public async Task<ErrorCode> RollbackDestroyItemAsync(Int64 itemId)
	{
		_logger.ZLogDebugWithPayload(new { ItemId = itemId }, "RollbackDestroyItem Start");

		try
		{
			var count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.UpdateAsync(new { IsDestroyed = false });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.RollbackDestroyItemFailUpdate, ItemId = itemId },
					"RollbackDestroyItemFailUpdate");

				return ErrorCode.RollbackDestroyItemFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new { ErrorCode = ErrorCode.RollbackDestroyItemFailException, ItemId = itemId },
				"RollbackDestroyItemFailException");

			return ErrorCode.RollbackDestroyItemFailException;
		}
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