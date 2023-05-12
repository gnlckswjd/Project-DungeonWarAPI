using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class UserService : IUserService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<UserService> _logger;
	private readonly OwnedItemFactory _ownedItemFactory;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public UserService(ILogger<UserService> logger, IOptions<DatabaseConfiguration> configurationOptions,
		OwnedItemFactory ownedItemFactory)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;
		_ownedItemFactory = ownedItemFactory;

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
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.CreateUserFailException },
				"CreateUserFailException");
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
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.CreateUserAttendanceFailInsert, GameUserId = gameUserId },
					"CreateUserAttendanceFailInsert");
				return ErrorCode.CreateUserAttendanceFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.CreateUserAttendanceFailException, GameUserId = gameUserId },
				"CreateUserAttendanceFailException");
			return ErrorCode.CreateUserAttendanceFailException;
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
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.RollbackCreateUserAttendanceFailDelete, GameUserId = gameUserId },
					"RollbackCreateUserAttendanceFailDelete");
				return ErrorCode.RollbackCreateUserAttendanceFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new
				{
					e, ErrorCode = ErrorCode.RollbackCreateUserAttendanceFailException, GameUserId = gameUserId
				},
				"RollbackCreateUserAttendanceFailException");
			return ErrorCode.RollbackCreateUserAttendanceFailException;
		}
	}

	public async Task<ErrorCode> CreateUserStageAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserClear Start");
		try
		{
			var count = await _queryFactory.Query("user_stage")
				.InsertAsync(new { GameUserId = gameUserId, ClearedStage = 0 });
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
			_logger.ZLogErrorWithPayload(new { GameUserId= gameUserId, ErrorCode = ErrorCode.CreateUserStageFailException },
				"CreateUserStageFailException");
			return ErrorCode.CreateUserStageFailException;
		}
	}

	public async Task<ErrorCode> RollbackCreateUserStageAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "RollbackCreateUserStage Start");
		try
		{
			var count = await _queryFactory.Query("user_stage").Where("GameUserId","=",gameUserId)
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
			_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, ErrorCode = ErrorCode.RollbackCreateUserStageFailException },
				"RollbackCreateUserStageFailException");
			return ErrorCode.RollbackCreateUserStageFailException;
		}
	}

	public async Task<ErrorCode> CreateUserItemAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserItem Start");

			var columns = new[] { "GameUserId", "ItemCode", "EnhancementCount", "ItemCount", "Attack", "Defense" };
			var data = new List<object[]>();

			var items = new List<OwnedItem>();
			items.Add(_ownedItemFactory.CreateOwnedItem(gameUserId, (int)ItemCode.SmallSword));
			items.Add(_ownedItemFactory.CreateOwnedItem(gameUserId, (int)ItemCode.OrdinaryHat));

			foreach (var item in items)
			{
				data.Add(new object[]
				{
					item.GameUserId,
					item.ItemCode,
					item.EnhancementCount,
					item.ItemCount,
					item.Attack,
					item.Defense
				});
			}

			var count = await _queryFactory.Query("owned_item").InsertAsync(columns, data);


			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.CreateUserItemFailInsert, GameUserId = gameUserId },
					"CreateUserItemFailInsert");
				return ErrorCode.CreateUserItemFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.CreateUserItemFailException, GameUserId = gameUserId },
				"CreateUserItemFailException");
			return ErrorCode.CreateUserItemFailException;
		}
	}

	public async Task<ErrorCode> RollbackCreateUserAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "RollbackUser Start");

			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId).DeleteAsync();

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
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.LoadUserDataFailException, PlayerId = playerId },
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
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.LoadUserItemsFailException, GameUserId = gameUserId },
				"LoadUserItemsFailException");
			return (ErrorCode.LoadUserItemsFailException, new List<OwnedItem>());
		}
	}
}