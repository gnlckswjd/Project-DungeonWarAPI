using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class ItemService: IItemService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<ItemService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public ItemService(ILogger<ItemService> logger, IOptions<DatabaseConfiguration> configurationOptions,
		MasterDataManager masterData)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;
		_masterData = masterData;

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


	public async Task<(ErrorCode, int itemCode, int enhancementCount)> LoadItemAsync(int gameUserId, long itemId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ItemId = itemId }, "LoadItem Start");
		try
		{
			var item = await _queryFactory.Query("owned_item").Where("GameUserId", "=", gameUserId)
				.Where("ItemId", "=", itemId)
				.FirstOrDefaultAsync<OwnedItem>();

			if (item == null || item.IsDestroyed)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadItemFailSelect, GameUserId = gameUserId, ItemId = itemId },
					"LoadItemFailSelect");
				return (ErrorCode.LoadItemFailSelect, 0, 0);
			}

			return (ErrorCode.None, item.ItemCode, item.EnhancementCount);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.LoadItemFailException, GameUserId = gameUserId, ItemId = itemId },
				"LoadItemFailException");
			return (ErrorCode.LoadItemFailException, 0, 0);
		}
	}

	public async Task<ErrorCode> UpdateGoldAsync(int gameUserId, int gold)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, Gold = gold }, "UpdateGold Start");

		try
		{
			var count = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.IncrementAsync("Gold", gold);
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
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.UpdateGoldFailException, GameUserId = gameUserId, Gold = gold },
				"UpdateGoldFailException");
			return ErrorCode.UpdateGoldFailException;
		}
	}

	public async Task<ErrorCode> UpdateEnhancementCountAsync(int gameUserId, long itemId, int enhancementCount)
	{
		_logger.ZLogDebugWithPayload(
			new { GameUserId = gameUserId, ItemId = itemId, EnhancementCount = enhancementCount },
			"UpdateEnhancementCount Start");

		try
		{
			var count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.UpdateAsync(new { EnhancementCount = enhancementCount + 1 });
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
				new
				{
					ErrorCode = ErrorCode.UpdateEnhancementCountFailException,
					GameUserId = gameUserId,
					ItemId = itemId,
					EnhancementCount = enhancementCount
				}, "UpdateEnhancementCountFailException");
			return ErrorCode.UpdateEnhancementCountFailException;
		}
	}

	public async Task<ErrorCode> DestroyItemAsync(int gameUserId, long itemId)
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
				new { ErrorCode = ErrorCode.DestroyItemFailException, GameUserId = gameUserId, ItemId = itemId },
				"DestroyItemFailException");

			return ErrorCode.DestroyItemFailException;
		}
	}

	public async Task<ErrorCode> InsertEnhancementHistoryAsync(int gameUserId, long itemId, int enhancementCount,
		bool isSuccess)
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
			_logger.ZLogErrorWithPayload(new
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

	public async Task<ErrorCode> RollbackUpdateMoneyAsync(int gameUserId, int gold)
	{
		return await UpdateGoldAsync(gameUserId, -gold);
	}


	public async Task<ErrorCode> RollbackUpdateEnhancementCountAsync(long itemId)
	{
		_logger.ZLogDebugWithPayload(
			new { ItemId = itemId }, "RollbackUpdateEnhancementCount Start");

		try
		{
			var count = await _queryFactory.Query("owned_item").Where("ItemId", "=", itemId)
				.DecrementAsync("EnhancementCount", 1);
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
			_logger.ZLogErrorWithPayload(
				new
				{
					ErrorCode = ErrorCode.RollbackUpdateEnhancementCountFailException,
					ItemId = itemId,
				}, "RollbackUpdateEnhancementCountFailException");
			return ErrorCode.RollbackUpdateEnhancementCountFailException;
		}
	}

	public async Task<ErrorCode> RollbackDestroyItemAsync(long itemId)
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
				new { ErrorCode = ErrorCode.RollbackDestroyItemFailException, ItemId = itemId },
				"RollbackDestroyItemFailException");

			return ErrorCode.RollbackDestroyItemFailException;
		}
	}

}