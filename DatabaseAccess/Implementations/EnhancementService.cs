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

public class EnhancementService : DatabaseAccessBase, IEnhancementService
{
	public EnhancementService(ILogger<EnhancementService> logger,QueryFactory queryFactory)
		:base(logger, queryFactory)
	{
		
	}


	public async Task<ErrorCode> VerifyEnoughGoldAsync(Int32 gameUserId, Int64 cost)
	{
		_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, Cost = cost }, "VerifyEnoughGold Start");

		try
		{
			var gold = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.Select("Gold")
				.FirstOrDefaultAsync<Int64>();

			if (gold == 0)
			{
				_logger.ZLogErrorWithPayload(new { GameUser = gameUserId, Cost = cost },
					"VerifyEnoughGoldFailSelect");
				return ErrorCode.VerifyEnoughGoldFailSelect;
			}

			if (gold < cost)
			{
				_logger.ZLogErrorWithPayload(new { GameUser = gameUserId, Cost = cost },
					"VerifyEnoughGoldFailNotEnoughGold");
				return ErrorCode.VerifyEnoughGoldFailNotEnoughGold;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { GameUserId = gameUserId }, "VerifyEnoughGoldFailException");
			return ErrorCode.VerifyEnoughGoldFailException;
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

	public async Task<ErrorCode> UpdateEnhancementResultAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount,
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



	private ErrorCode ValidateItemForEnhancement(Int32 gameUserId, OwnedItem item)
	{
		if (item.GameUserId != gameUserId)
		{
			return ErrorCode.LoadItemFailWrongGameUser;
		}

		if (item.IsDestroyed == true)
		{
			return ErrorCode.LoadItemFailDestroyed;
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