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

public class LoginRewardService : ILoginRewardService
{

	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<LoginRewardService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public LoginRewardService(ILogger<LoginRewardService> logger, IOptions<DatabaseConfiguration> configurationOptions,
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



	public async Task<(ErrorCode, DateTime lastLoginDate, short attendanceCount)> UpdateLoginDateAsync(
		int gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "UpdateLoginAndGetAttendance");

		try
		{
			var userData = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId).FirstOrDefaultAsync<UserData>();

			if (userData == null)
			{
				_logger.ZLogErrorWithPayload(
					new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailUserNotFound },
					"UpdateLoginDateFailUserNotFound");
				return (ErrorCode.UpdateLoginDateFailUserNotFound, default, default);
			}

			var lastLoginDate = userData.LastLoginDate.Date;
			var today = DateTime.Now.Date;

			short attendanceCount = userData.AttendanceCount;

			if (lastLoginDate == today)
			{
				_logger.ZLogErrorWithPayload(
					new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailAlreadyReceived },
					"UpdateLoginDateFailAlreadyReceived");
				return (ErrorCode.UpdateLoginDateFailAlreadyReceived, default, default);
			}

			if (lastLoginDate == today.AddDays(-1) && lastLoginDate.Date.Month == today.Month)
			{
				attendanceCount++;
			}
			else
			{
				attendanceCount = 1;
			}


			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId)
				.UpdateAsync(new { LastLoginDate = today, AttendanceCount = attendanceCount });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailUpdate },
					"UpdateLoginDateFailUpdate");
				return (ErrorCode.UpdateLoginDateFailUpdate, default, default);
			}

			return (ErrorCode.None, lastLoginDate, attendanceCount);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { GameUserId = gameUserId, ErrorCode = ErrorCode.UpdateLoginDateFailException },
				"UpdateLoginDateFailException");
			return (ErrorCode.UpdateLoginDateFailException, default, default);
		}
	}

	public async Task<ErrorCode> CreateAttendanceRewardMailAsync(int gameUserId, AttendanceReward reward)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateAttendanceMail Start");
		long mailId = 0;
		try
		{
			mailId = await _queryFactory.Query("mail").InsertGetIdAsync<long>(
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

	public async Task<ErrorCode> RollbackLoginDateAsync(int gameUserId, DateTime lastLoginDate, short attendanceCount)
	{
		_logger.ZLogDebugWithPayload(
			new { GameUserId = gameUserId, LastLogin = lastLoginDate, AttendanceCount = attendanceCount },
			"RollbackLoginDate Start");
		try
		{
			var count = await _queryFactory.Query("user_data")
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


	private async Task RollbackCreateMailAsync(long mailId)
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
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateMailFailException, MailId = mailId },
				"RollbackCreateMailFailException");
		}
	}
}