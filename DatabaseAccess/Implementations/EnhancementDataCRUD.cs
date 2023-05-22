using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class EnhancementDataCRUD : DatabaseAccessBase, IEnhancementDataCRUD
{
	public EnhancementDataCRUD(ILogger<EnhancementDataCRUD> logger,QueryFactory queryFactory)
		:base(logger, queryFactory)
	{
		
	}

	public async Task<ErrorCode> UpdateEnhancementResultAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Int32 attributeCode, Int32 attack, Int32 defense)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ItemId = itemId, EnhancementCount = attributeCode }, "UpdateEnhancementCount Start");

		try
		{
			var count = await UpdateEnhancedItemAsync(itemId, enhancementCount, attributeCode, attack, defense);

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateEnhancementCountFailUpdate, GameUserId = gameUserId, ItemId = itemId, EnhancementCount = enhancementCount },
					"UpdateEnhancementCountFailUpdate");

				return ErrorCode.UpdateEnhancementCountFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.UpdateEnhancementCountFailException, GameUserId = gameUserId, ItemId = itemId, EnhancementCount = attributeCode }, 
				"UpdateEnhancementCountFailException");

			return ErrorCode.UpdateEnhancementCountFailException;
		}
	}


	public async Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount,
		Boolean isSuccess)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ItemId = itemId, EnhancementCount = enhancementCount },
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
				_logger.ZLogErrorWithPayload(new {
						ErrorCode = ErrorCode.InsertEnhancementHistoryFailInsert,
						GameUserId = gameUserId,
						ItemId = itemId, 
						EnhancementCountBefore = enhancementCount,
						EnhancementCountAfter = enhancementCountAfter, 
						IsSuccess = isSuccess
					},
					"InsertEnhancementHistoryFailInsert");

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
			}, 
				"InsertEnhancementHistoryFailException");
			return ErrorCode.InsertEnhancementHistoryFailException;
		}
	}




	public async Task<ErrorCode> RollbackUpdateEnhancementCountAsync(Int64 itemId, Int32 attributeCode, Int32 attack, Int32 defense, Int32 enhancementCount)
	{
		_logger.ZLogDebugWithPayload(new { ItemId = itemId }, "RollbackUpdateEnhancementCount Start");

		try
		{
			Int32 count;
			if (attributeCode == (int)ItemAttributeCode.Weapon)
			{
				count = await _queryFactory.Query("owned_item")
					.Where("ItemId", "=", itemId)
					.UpdateAsync(new{ EnhancementCount=enhancementCount , Attack= attack});
			}
			else
			{
				count = await _queryFactory.Query("owned_item")
					.Where("ItemId", "=", itemId)
					.UpdateAsync(new{ EnhancementCount = enhancementCount, Defense = defense });
			}
			
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackUpdateEnhancementCountFailUpdate, ItemId = itemId, },
					"RollbackUpdateEnhancementCountFailUpdate");

				return ErrorCode.RollbackUpdateEnhancementCountFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackUpdateEnhancementCountFailException, ItemId = itemId, }, 
				"RollbackUpdateEnhancementCountFailException");

			return ErrorCode.RollbackUpdateEnhancementCountFailException;
		}
	}

	private async Task<Int32> UpdateEnhancedItemAsync(Int64 itemId, Int32 enhancementCount, Int32 attributeCode, Int32 attack, Int32 defense
		)
	{
		Int32 count;
		if (attributeCode == (int)ItemAttributeCode.Weapon)
		{
			count = await _queryFactory.Query("owned_item")
				.Where("ItemId", "=", itemId)
				.UpdateAsync(new { EnhancementCount = enhancementCount + 1, Attack = attack });
		}
		else
		{
			count = await _queryFactory.Query("owned_item")
				.Where("ItemId", "=", itemId)
				.UpdateAsync(new { EnhancementCount = enhancementCount + 1, defense });
		}

		return count;
	}
}