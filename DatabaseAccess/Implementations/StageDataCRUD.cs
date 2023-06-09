﻿using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class StageDataCRUD : DatabaseAccessBase, IStageDataCRUD
{

	public StageDataCRUD(ILogger<StageDataCRUD> logger, QueryFactory queryFactory) : base(logger, queryFactory)
	{

	}

	public async Task<(ErrorCode, Int32)> LoadStageListAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadStageList Start");

		try
		{
			var userStage = await _queryFactory.Query("user_stage")
				.Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserStage>();

			if (userStage == null)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadUserStageFailSelect, GameUserId = gameUserId },
					"LoadUserStageFailSelect");

				return (ErrorCode.LoadUserStageFailSelect, 0);
			}

			return (ErrorCode.None, userStage.MaxClearedStage);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadUserStageFailSelect, GameUserId = gameUserId },
				"LoadUserStageFailException");

			return (ErrorCode.LoadUserStageFailException, 0);
		}
	}

	public async Task<ErrorCode> RollbackUpdateExpAsync(Int32 gameUserId, Int32 level, Int32 exp)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, UserLevel = level, Exp = exp },
			"RollbackUpdateExp Start");

		try
		{
			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId)
				.UpdateAsync(new { UserLevel = level, Exp = exp });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { Errorcode = ErrorCode.RollbackUpdateExpFailUpdate, GameUserId = gameUserId, Level = level, Exp = exp }, 
					"RollbackUpdateExpFailUpdate");

				return ErrorCode.RollbackUpdateExpFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { Errorcode = ErrorCode.RollbackUpdateExpFailException, GameUserId = gameUserId, Level = level, Exp = exp }, 
				"RollbackUpdateExpFailException");

			return ErrorCode.RollbackUpdateExpFailException;
		}
	}

	public async Task<(ErrorCode, Boolean isIncrement)> IncreaseMaxClearedStageAsync(Int32 gameUserId, Int32 clearLevel)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ClearStageLevel = clearLevel },
			"UpdateMaxClearedStage Start");

		try
		{
			var count = await _queryFactory.Query("user_stage")
				.Where("GameUserId", "=", gameUserId)
				.Where("MaxClearedStage", "<", clearLevel)
				.IncrementAsync("MaxClearedStage", 1);

			if (count == 0)
			{
				return (ErrorCode.None, false);
			}

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateMaxClearedStageFailIncrement, GameUserId = gameUserId },
					"UpdateMaxClearedStageFailIncrement");

				return (ErrorCode.UpdateMaxClearedStageFailIncrement, false);
			}

			return (ErrorCode.None, true);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.UpdateMaxClearedStageFailException, GameUserId = gameUserId },
				"UpdateMaxClearedStageFailException");

			return (ErrorCode.UpdateMaxClearedStageFailException, false);
		}
	}

	public async Task<ErrorCode> RollbackIncreaseMaxClearedStageAsync(Int32 gameUserId, Boolean isIncrement)
	{
		if (isIncrement == false)
		{
			return ErrorCode.None;
		}
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "RollbackUpdateMaxClearedStage Start");

		try
		{
			var count = await _queryFactory.Query("user_stage")
				.Where("GameUserId", "=", gameUserId)
				.DecrementAsync("MaxClearedStage", 1);

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackUpdateMaxClearedStageFailDecrement, GameUserId = gameUserId },
					"RollbackUpdateMaxClearedStageFailDecrement");

				return ErrorCode.RollbackUpdateMaxClearedStageFailDecrement;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackUpdateMaxClearedStageFailException, GameUserId = gameUserId },
				"RollbackUpdateMaxClearedStageFailException");

			return ErrorCode.RollbackUpdateMaxClearedStageFailException;
		}
	}
}