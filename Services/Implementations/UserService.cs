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

public class UserService : IUserService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<UserService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public UserService(ILogger<UserService> logger, IOptions<DatabaseConfiguration> configurationOptions,
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
	public async Task<(ErrorCode, int)> CreateUserAsync(int playerId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { PlayerId = playerId }, "CreateUser Start");

			var gameUserId = await _queryFactory.Query("user_data")
				.InsertGetIdAsync<int>(new { PlayerId = playerId });

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

	public async Task<ErrorCode> CreateUserItemAsync(int gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserItem Start");
			var columns = new[] { "GameUserId", "ItemCode", "EnhancementCount", "ItemCount" };
			var data = new[]
			{
				new object[] { gameUserId, 2, 0, 1 },
				new object[] { gameUserId, 3, 0, 1 }
			};

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

	public async Task<ErrorCode> RollbackCreateUserAsync(int gameUserId)
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

	public async Task<(ErrorCode, UserData)> LoadUserDataAsync(int playerId)
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

	public async Task<(ErrorCode, List<OwnedItem>)> LoadUserItemsAsync(int gameUserId)
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
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadUserItemsFailSelect, GameUserId = gameUserId },
					"LoadUserItemsFailSelect");
				return (ErrorCode.LoadUserItemsFailSelect, null);
			}

			return (ErrorCode.None, items.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.LoadUserItemsFailException, GameUserId = gameUserId },
				"LoadUserItemsFailException");
			return (ErrorCode.LoadUserItemsFailException, null);
		}
	}

}