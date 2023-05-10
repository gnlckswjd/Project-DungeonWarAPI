using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class AttendanceRewardService : IAttendanceRewardService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<AttendanceRewardService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public AttendanceRewardService(ILogger<AttendanceRewardService> logger,
		IOptions<DatabaseConfiguration> configurationOptions,
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


	public async Task<(ErrorCode, Int32)> LoadAttendanceCountAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadLoginDate");

		try
		{
			var userAttendance= await _queryFactory.Query("user_attendance").Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserAttendance>();

			if (userAttendance == null)
			{
				_logger.ZLogErrorWithPayload(
					new { GameUserId = gameUserId, ErrorCode = ErrorCode.LoadAttendanceCountFailSelect },
					"LoadAttendanceCountFailSelect");
				return (ErrorCode.LoadAttendanceCountFailSelect, default);
			}

			var (_, attendanceCount) = CalcAttendanceDate(gameUserId, userAttendance);

			

			return (ErrorCode.None, attendanceCount);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new { ErrorCode = ErrorCode.LoadAttendanceCountFailException, GameUserId = gameUserId },
				"LoadAttendanceCountFailSelect");
			return (ErrorCode.LoadAttendanceCountFailException, 0);
		}
	}

	public async Task<(ErrorCode, DateTime lastLoginDate, Int16 attendanceCount)> UpdateLoginDateAsync(
		Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "UpdateLoginAndGetAttendance");

		try
		{
			var userAttendance = await _queryFactory.Query("user_attendance")
				.Where("GameUserId", "=", gameUserId).FirstOrDefaultAsync<UserAttendance>();

			if (userAttendance == null)
			{
				_logger.ZLogErrorWithPayload(
					new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailUserNotFound },
					"UpdateLoginDateFailUserNotFound");
				return (ErrorCode.UpdateLoginDateFailUserNotFound, default, default);
			}

			var(errorCode, attendanceCount) = CalcAttendanceDate(gameUserId, userAttendance);

			if (errorCode != ErrorCode.None)
			{
				return (errorCode, userAttendance.LastLoginDate, attendanceCount);
			}


			var count = await _queryFactory.Query("user_attendance")
				.Where("GameUserId", "=", gameUserId)
				.UpdateAsync(new { LastLoginDate = DateTime.Today.Date, AttendanceCount = attendanceCount });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailUpdate },
					"UpdateLoginDateFailUpdate");
				return (ErrorCode.UpdateLoginDateFailUpdate, default, default);
			}

			return (ErrorCode.None, userAttendance.LastLoginDate, attendanceCount);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailException },
				"UpdateLoginDateFailException");
			return (ErrorCode.UpdateLoginDateFailException, default, default);
		}
	}

	public async Task<ErrorCode> CreateAttendanceRewardMailAsync(Int32 gameUserId, AttendanceReward reward)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateAttendanceMail Start");
		long mailId = 0;
		try
		{
			mailId = await _queryFactory.Query("mail").InsertGetIdAsync<Int64>(
				new
				{
					GameUserId = gameUserId,
					Title = reward.AttendanceCount.ToString() + "일 출석 보상",
					Contents = reward.AttendanceCount.ToString() + "일 출석 보상 지급 안내",
					isRead = false,
					isReceived = false,
					isInApp = false,
					isRemoved = false,
				});
			if (mailId < 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.CreateAttendanceMailFailInsertMail,
						GameUserId = gameUserId,
						MailId = mailId
					},
					"CreateAttendanceMailFailInsertMail");
				return ErrorCode.CreateAttendanceMailFailInsertMail;
			}

			var count = await _queryFactory.Query("mail_item").InsertAsync(new
			{
				MailId = mailId,
				reward.ItemCode,
				reward.ItemCount,
			});

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.CreateAttendanceMailFailInsertItem,
						GameUserId = gameUserId,
						MailId = mailId
					},
					"CreateAttendanceMailFailInsertItem");

				await RollbackCreateMailAsync(mailId);

				return ErrorCode.CreateAttendanceMailFailInsertItem;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new
				{
					ErrorCode = ErrorCode.CreateAttendanceMailFailException,
					GameUserId = gameUserId,
					MailId = mailId
				},
				"CreateAttendanceMailFailException");
			await RollbackCreateMailAsync(mailId);
			return ErrorCode.CreateAttendanceMailFailException;
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
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackLoginDateFailUpdate,
						LastLoginDate = lastLoginDate,
						AttendnceCount = attendanceCount
					},
					"RollbackLoginDateFailUpdate");
				return ErrorCode.RollbackLoginDateFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e,
				new
				{
					ErrorCode = ErrorCode.RollbackLoginDateFailException,
					LastLoginDate = lastLoginDate,
					AttendnceCount = attendanceCount
				},
				"RollbackLoginDateFailException");
			return ErrorCode.RollbackLoginDateFailException;
		}
	}


	private async Task RollbackCreateMailAsync(Int64 mailId)
	{
		if (mailId == 0)
		{
			return;
		}

		try
		{
			var count = await _queryFactory.Query("mail").Where("MailId", "=", mailId).DeleteAsync();

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.RollbackCreateMailFailDelete, MailId = mailId },
					"RollbackCreateMailFailDelete");
			}
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				e, new { ErrorCode = ErrorCode.RollbackCreateMailFailException, MailId = mailId },
				"RollbackCreateMailFailException");
		}
	}

	private (ErrorCode,Int16 attendanceCount) CalcAttendanceDate(Int32 gameUserId, UserAttendance userAttendance)
	{
		var lastLoginDate = userAttendance.LastLoginDate.Date;
		var today = DateTime.Now.Date;

		short attendanceCount = userAttendance.AttendanceCount;

		if (lastLoginDate == today)
		{
			_logger.ZLogErrorWithPayload(
				new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailAlreadyReceived },
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