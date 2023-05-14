using System.Data;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.Database.Game;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class EnhancementService : IEnhancementService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<EnhancementService> _logger;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public EnhancementService(ILogger<EnhancementService> logger, IOptions<DatabaseConfiguration> configurationOptions)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;

		_databaseConnection = new MySqlConnection(configurationOptions.Value.GameDatabase);
		_databaseConnection.Open();

		var compiler = new MySqlCompiler();
		_queryFactory = new QueryFactory(_databaseConnection, compiler);
	}


	public void Dispose()
	{
		_databaseConnection.Dispose();
		//_queryFactory.Dispose();
	}

	public async Task<ErrorCode> ValidateEnoughGoldAsync(Int32 gameUserId, Int64 cost)
	{
		_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, Cost = cost }, "ValidateEnoughGold Start");

		try
		{
			var gold = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.Select("Gold")
				.FirstOrDefaultAsync<Int64>();

			if (gold == 0)
			{
				_logger.ZLogErrorWithPayload(new { GameUser = gameUserId, Cost = cost },
					"ValidateEnoughGoldFailSelect");
				return ErrorCode.ValidateEnoughGoldFailSelect;
			}

			if (gold < cost)
			{
				_logger.ZLogErrorWithPayload(new { GameUser = gameUserId, Cost = cost },
					"ValidateEnoughGoldFailNotEnoughGold");
				return ErrorCode.ValidateEnoughGoldFailNotEnoughGold;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { GameUserId = gameUserId }, "ValidateEnoughGoldFailException");
			return ErrorCode.ValidateEnoughGoldFailException;
		}
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

	public async Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, Gold = gold }, "UpdateGold Start");

		try
		{
			var count = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.DecrementAsync("Gold", gold);
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.UpdateGoldFailIncrease, GameUserId = gameUserId, Gold = gold },
					"UpdateGoldFailIncrease");
				return ErrorCode.UpdateGoldFailIncrease;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.UpdateGoldFailException, GameUserId = gameUserId, Gold = gold },
				"UpdateGoldFailException");
			return ErrorCode.UpdateGoldFailException;
		}
	}

	public async Task<ErrorCode> UpdateEnhancementResultAsync(int gameUserId, long itemId, int enhancementCount,
		int attributeCode, int attack, int defense)
	{
		_logger.ZLogDebugWithPayload(
			new { GameUserId = gameUserId, ItemId = itemId, EnhancementCount = attributeCode },
			"UpdateEnhancementCount Start");

		try
		{
			var count = await UpdateEnhancedItemAsync(itemId, enhancementCount, attributeCode, attack, defense);

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.UpdateEnhancementCountFailUpdate,
						GameUserId = gameUserId,
						ItemId = itemId,
						EnhancementCount = enhancementCount
					}, "UpdateEnhancementCountFailUpdate");
				return ErrorCode.UpdateEnhancementCountFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new
				{
					ErrorCode = ErrorCode.UpdateEnhancementCountFailException,
					GameUserId = gameUserId,
					ItemId = itemId,
					EnhancementCount = attributeCode
				}, "UpdateEnhancementCountFailException");
			return ErrorCode.UpdateEnhancementCountFailException;
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

	public async Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount,
		Boolean isSuccess)
	{
		_logger.ZLogDebugWithPayload(
			new { GameUserId = gameUserId, ItemId = itemId, EnhancementCount = enhancementCount },
			"InsertEnhancementHistory Start");

		var enhancementCountAfter = enhancementCount;
		if (isSuccess)
		{
			enhancementCountAfter = enhancementCount + 1;
		}

		try
		{
			var count = await _queryFactory.Query("enhancement_history").InsertAsync(new
			{
				GameUserId = gameUserId,
				ItemId = itemId,
				EnhancedCountBefore = enhancementCount,
				EnhancedCountAfter = enhancementCountAfter,
				IsSuccess = isSuccess
			});

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new
				{
					ErrorCode = ErrorCode.InsertEnhancementHistoryFailInsert,
					GameUserId = gameUserId,
					ItemId = itemId,
					EnhancementCountBefore = enhancementCount,
					EnhancementCountAfter = enhancementCountAfter,
					IsSuccess = isSuccess
				}, "InsertEnhancementHistoryFailInsert");
				return ErrorCode.InsertEnhancementHistoryFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new
			{
				ErrorCode = ErrorCode.InsertEnhancementHistoryFailException,
				GameUserId = gameUserId,
				ItemId = itemId,
				EnhancementCountBefore = enhancementCount,
				EnhancementCountAfter = enhancementCountAfter,
				IsSuccess = isSuccess
			}, "InsertEnhancementHistoryFailException");
			return ErrorCode.InsertEnhancementHistoryFailException;
		}
	}

	public async Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, Gold = -gold }, "RollbackUpdateMoneyAsync");
		return await UpdateGoldAsync(gameUserId, -gold);
	}


	public async Task<ErrorCode> RollbackUpdateEnhancementCountAsync(long itemId, int attributeCode, int attack,
		int defense, int enhancementCount)
	{
		_logger.ZLogDebugWithPayload(
			new { ItemId = itemId }, "RollbackUpdateEnhancementCount Start");

		try
		{
			Int32 count;
			if (attributeCode == (int)ItemAttributeCode.Weapon)
			{
				count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
					.UpdateAsync(new{ EnhancementCount=enhancementCount , Attack= attack});
			}
			else
			{
				count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
					.UpdateAsync(new{ EnhancementCount = enhancementCount, Defense = defense });
			}
			
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackUpdateEnhancementCountFailUpdate,

						ItemId = itemId,
					}, "RollbackUpdateEnhancementCountFailUpdate");
				return ErrorCode.RollbackUpdateEnhancementCountFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.RollbackUpdateEnhancementCountFailException,
					ItemId = itemId,
				}, "RollbackUpdateEnhancementCountFailException");
			return ErrorCode.RollbackUpdateEnhancementCountFailException;
		}
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

	private ErrorCode ValidateItemForEnhancement(Int32 gameUserId, OwnedItem item)
	{
		if (item.GameUserId != gameUserId)
		{
			return ErrorCode.LoadItemFailWrongGameUser;
		}

		if (item.IsDestroyed == true)
		{
			return ErrorCode.LoadItemFailisDestroyed;
		}

		return ErrorCode.None;
	}

	private async Task<Int32> UpdateEnhancedItemAsync(Int64 itemId, Int32 enhancementCount, Int32 attributeCode, Int32 attack, Int32 defense
		)
	{
		Int32 count;
		if (attributeCode == (int)ItemAttributeCode.Weapon)
		{
			count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.UpdateAsync(new { EnhancementCount = enhancementCount + 1, Attack = attack });
		}
		else
		{
			count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.UpdateAsync(new { EnhancementCount = enhancementCount + 1, defense });
		}

		return count;
	}
}