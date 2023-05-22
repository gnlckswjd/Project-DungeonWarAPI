using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.Database.Game;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class UserDataCRUD : DatabaseAccessBase, IUserDataCRUD
{
	private readonly OwnedItemFactory _ownedItemFactory;

	public UserDataCRUD(ILogger<UserDataCRUD> logger, QueryFactory queryFactory, OwnedItemFactory ownedItemFactory)
		: base(logger, queryFactory)
	{
		_ownedItemFactory = ownedItemFactory;
	}


	public async Task<(ErrorCode, UserData)> LoadUserDataAsync(Int32 playerId)
	{
		_logger.ZLogDebugWithPayload(new { PlayerId = playerId }, "LoadUserData Start");

		try
		{
			var userData = await _queryFactory.Query("user_data")
				.Where("PlayerId", "=", playerId).FirstOrDefaultAsync<UserData>();
			if (userData == null)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadUserDataFailSelect, PlayerId = playerId },
					"ErrorCode.LoadUserDataFailSelect");

				return (ErrorCode.LoadUserDataFailSelect, null);
			}

			return (ErrorCode.None, userData);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadUserDataFailException, PlayerId = playerId },
				"LoadUserDataFailException");

			return (ErrorCode.LoadUserDataFailException, null);
		}
	}

	public async Task<(ErrorCode, List<OwnedItem>)> LoadUserItemsAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadUserItems Start");
		try
		{
			var items = await _queryFactory.Query("owned_item")
				.Where("GameUserId", "=", gameUserId)
				.Where("isDestroyed", "=", false)
				.GetAsync<OwnedItem>();

			if (!items.Any())
			{
				return (ErrorCode.None, new List<OwnedItem>());
			}

			return (ErrorCode.None, items.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadUserItemsFailException, GameUserId = gameUserId },
				"LoadUserItemsFailException");

			return (ErrorCode.LoadUserItemsFailException, new List<OwnedItem>());
		}
	}


	public async Task<(ErrorCode, Int64 gold)> LoadGoldAsync(Int32 gameUserId)
	{
		_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId }, "LoadGold Start");

		try
		{
			var gold = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId)
				.Select("Gold")
				.FirstOrDefaultAsync<Int64>();

			if (gold == 0)
			{
				_logger.ZLogErrorWithPayload(new { GameUser = gameUserId}, "LoadGoldFailSelect");

				return (ErrorCode.LoadGoldFailSelect, 0);
			}


			return (ErrorCode.None, gold);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { GameUserId = gameUserId }, "LoadGoldFailException");

			return (ErrorCode.LoadGoldFailException, 0);
		}
	}


	public async Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, Gold = gold }, "UpdateGold Start");

		try
		{
			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId)
				.DecrementAsync("Gold", gold);

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateGoldFailIncrease, GameUserId = gameUserId, Gold = gold }, 
					"UpdateGoldFailIncrease");

				return ErrorCode.UpdateGoldFailIncrease;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.UpdateGoldFailException, GameUserId = gameUserId, Gold = gold }, 
				"UpdateGoldFailException");

			return ErrorCode.UpdateGoldFailException;
		}
	}

	public async Task<(ErrorCode, Int32)> CreateUserAsync(Int32 playerId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { PlayerId = playerId }, "CreateUser Start");

			var gameUserId = await _queryFactory.Query("user_data")
				.InsertGetIdAsync<Int32>(new { PlayerId = playerId });

			_logger.ZLogInformationWithPayload(new { GameUserId = gameUserId }, "CreateUser Success");

			return (ErrorCode.None, gameUserId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.CreateUserFailException }, "CreateUserFailException");

			return (ErrorCode.CreateUserFailException, 0);
		}
	}

	public async Task<ErrorCode> CreateUserAttendanceAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserAttendance Start");

			var count = await _queryFactory.Query("user_attendance")
				.InsertAsync(new
				{
					GameUserId = gameUserId, AttendanceCount = 0,
					LastLoginDate = DateTime.Today.AddDays(-1)
				});

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateUserAttendanceFailInsert, GameUserId = gameUserId }, 
					"CreateUserAttendanceFailInsert");

				return ErrorCode.CreateUserAttendanceFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.CreateUserAttendanceFailException, GameUserId = gameUserId }, 
				"CreateUserAttendanceFailException");

			return ErrorCode.CreateUserAttendanceFailException;
		}
	}


	public async Task<ErrorCode> CreateUserStageAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserClear Start");
		try
		{
			var count = await _queryFactory.Query("user_stage")
				.InsertAsync(new { GameUserId = gameUserId, MaxClearedStage = 0 });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.CreateUserStageFailInsert }, 
					"CreateUserStageFailInsert");

				return ErrorCode.CreateUserStageFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.CreateUserStageFailException }, 
				"CreateUserStageFailException");

			return ErrorCode.CreateUserStageFailException;
		}
	}


	public async Task<(ErrorCode, Int32 existingLevel, Int32 existingExp)> UpdateExpAsync(Int32 gameUserId, Int32 exp)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, Exp = exp }, "UpdateExp Start");

		try
		{
			var userData = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserData>();

			if (userData == null)
			{
				_logger.ZLogErrorWithPayload(new { ErroCode = ErrorCode.UpdateExpFailSelect, GameUserId = gameUserId },
					"UpdateExpFailSelect");

				return (ErrorCode.UpdateExpFailSelect, 0, 0);
			}

			Int32 levelUpCount = (userData.Exp + exp) / 1000;
			Int32 remainingExp = (userData.Exp + exp) % 1000;

			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId)
				.UpdateAsync(new { UserLevel = userData.UserLevel + levelUpCount, Exp = remainingExp });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErroCode = ErrorCode.UpdateExpFailUpdate, GameUserId = gameUserId }, 
					"UpdateExpFailUpdate");

				return (ErrorCode.UpdateExpFailUpdate, 0, 0);
			}

			return (ErrorCode.None, userData.UserLevel, userData.Exp);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErroCode = ErrorCode.UpdateExpFailException, GameUserId = gameUserId }, 
				"UpdateExpFailException");

			return (ErrorCode.UpdateExpFailException, 0, 0);
		}
	}

	public async Task<ErrorCode> RollbackCreateUserAttendanceAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserAttendance Start");

			var count = await _queryFactory.Query("user_attendance")
				.Where("GameUserId", "=", gameUserId)
				.DeleteAsync();

			if (count != 0)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateUserAttendanceFailDelete, GameUserId = gameUserId },
					"RollbackCreateUserAttendanceFailDelete");
				
				return ErrorCode.RollbackCreateUserAttendanceFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { e, ErrorCode = ErrorCode.RollbackCreateUserAttendanceFailException, GameUserId = gameUserId },
				"RollbackCreateUserAttendanceFailException");

			return ErrorCode.RollbackCreateUserAttendanceFailException;
		}
	}

	public async Task<ErrorCode> RollbackCreateUserStageAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "RollbackCreateUserStage Start");
		try
		{
			var count = await _queryFactory.Query("user_stage").Where("GameUserId", "=", gameUserId)
				.DeleteAsync();
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.RollbackCreateUserStageFailDelete },
					"RollbackCreateUserStageFailDelete");

				return ErrorCode.RollbackCreateUserStageFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { GameUserId = gameUserId, ErrorCode = ErrorCode.RollbackCreateUserStageFailException },
				"RollbackCreateUserStageFailException");

			return ErrorCode.RollbackCreateUserStageFailException;
		}
	}


	public async Task<ErrorCode> RollbackCreateUserAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "RollbackUser Start");

			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId)
				.DeleteAsync();

			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateUserDataFailDelete }, 
					"RollbackCreateUserDataFailDelete");

				return ErrorCode.RollbackCreateUserDataFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackCreateUserDataFailException }, 
				"RollbackCreateUserDataFailException");

			return ErrorCode.RollbackCreateUserDataFailException;
		}
	}

	public async Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, Gold = -gold }, "RollbackUpdateMoneyAsync");

		return await UpdateGoldAsync(gameUserId, -gold);
	}
}