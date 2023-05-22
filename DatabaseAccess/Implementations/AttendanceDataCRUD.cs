using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Game;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class AttendanceDataCRUD : DatabaseAccessBase, IAttendanceDataCRUD
{
	public AttendanceDataCRUD(ILogger<AttendanceDataCRUD> logger, QueryFactory queryFactory) 
		:base(logger,queryFactory)
	{
		
	}


	public async Task<(ErrorCode, Int32)> LoadAttendanceCountAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadLoginDate");

		try
		{
			var userAttendance = await _queryFactory.Query("user_attendance").Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserAttendance>();

			if (userAttendance == null)
			{
				_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.LoadAttendanceCountFailSelect },
					"LoadAttendanceCountFailSelect");

				return (ErrorCode.LoadAttendanceCountFailSelect, default);
			}

			var (_, attendanceCount) = CalcAttendanceDate(gameUserId, userAttendance);


			return (ErrorCode.None, attendanceCount);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadAttendanceCountFailException, GameUserId = gameUserId },
				"LoadAttendanceCountFailSelect");

			return (ErrorCode.LoadAttendanceCountFailException, 0);
		}
	}

	public async Task<(ErrorCode, DateTime lastLoginDate, Int16 attendanceCount)> UpdateLoginDateAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "UpdateLoginAndGetAttendance");

		try
		{
			var userAttendance = await _queryFactory.Query("user_attendance")
				.Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserAttendance>();

			if (userAttendance == null)
			{
				_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailUserNotFound },
					"UpdateLoginDateFailUserNotFound");

				return (ErrorCode.UpdateLoginDateFailUserNotFound, default, default);
			}

			var (errorCode, attendanceCount) = CalcAttendanceDate(gameUserId, userAttendance);
			if (errorCode != ErrorCode.None)
			{
				return (errorCode, userAttendance.LastLoginDate, attendanceCount);
			}

			var count = await _queryFactory.Query("user_attendance")
				.Where("GameUserId", "=", gameUserId)
				.UpdateAsync(new { LastLoginDate = DateTime.Today.Date, AttendanceCount = attendanceCount });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailUpdate },
					"UpdateLoginDateFailUpdate");

				return (ErrorCode.UpdateLoginDateFailUpdate, default, default);
			}

			return (ErrorCode.None, userAttendance.LastLoginDate, attendanceCount);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailException },
				"UpdateLoginDateFailException");

			return (ErrorCode.UpdateLoginDateFailException, default, default);
		}
	}


	public async Task<ErrorCode> RollbackLoginDateAsync(Int32 gameUserId, DateTime lastLoginDate, Int16 attendanceCount)
	{
		_logger.ZLogDebugWithPayload(
			new { GameUserId = gameUserId, LastLogin = lastLoginDate, AttendanceCount = attendanceCount },
			"RollbackLoginDate Start");
		try
		{
			var count = await _queryFactory.Query("user_attendance")
				.Where("GameUserId", "=", gameUserId)
				.UpdateAsync(new { LastLoginDate = lastLoginDate, AttendanceCount = attendanceCount - 1 });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackLoginDateFailUpdate, LastLoginDate = lastLoginDate, AttendnceCount = attendanceCount }, 
					"RollbackLoginDateFailUpdate");

				return ErrorCode.RollbackLoginDateFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackLoginDateFailException, LastLoginDate = lastLoginDate, AttendnceCount = attendanceCount },
				"RollbackLoginDateFailException");

			return ErrorCode.RollbackLoginDateFailException;
		}
	}

	private (ErrorCode, Int16 attendanceCount) CalcAttendanceDate(Int32 gameUserId, UserAttendance userAttendance)
	{
		var lastLoginDate = userAttendance.LastLoginDate.Date;
		var today = DateTime.Now.Date;

		short attendanceCount = userAttendance.AttendanceCount;

		if (lastLoginDate == today)
		{
			_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailAlreadyReceived },
				"UpdateLoginDateFailAlreadyReceived");

			return (ErrorCode.UpdateLoginDateFailAlreadyReceived, attendanceCount);
		}

		if (lastLoginDate == today.AddDays(-1) && lastLoginDate.Date.Month == today.Month)
		{
			attendanceCount++;
		}
		else
		{
			attendanceCount = 1;
		}

		return (ErrorCode.None, attendanceCount);
	}
}