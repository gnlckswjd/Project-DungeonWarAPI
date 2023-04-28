using DungeonWarAPI.ModelConfiguration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace DungeonWarAPI.Services;

public class GameDatabase : IGameDatabase
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<GameDatabase> _logger;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public GameDatabase(ILogger<GameDatabase> logger, IOptions<DatabaseConfiguration> configurationOptions)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;

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

	public async Task<ErrorCode> CreateUserAsync(Byte[] guid)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new {Guid = guid}, "CreateUser Start");
			Console.WriteLine($"[Create UserData] guid: {guid}");

			var count = await _queryFactory.Query("user_data")
				.InsertAsync(new { AccountId = guid });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload( new {ErrorCode = ErrorCode.CreateUserFailInsert }, "CreateUserFailInsert");
				return ErrorCode.CreateUserFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateUserFailException }, "CreateUserFailException");
			return ErrorCode.CreateUserFailException;
		}
	}

	public async Task<ErrorCode> CreateUserItemAsync(Byte[] guid)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new{Guid = guid}, "CreateUserItem Start");
			var columns = new[] { "AccountId", "ItemCode", "EnhancementValue", "ItemCount" };
			var data = new[]
			{
				new object[] { guid, 5, 0, 5000 },
				new object[] { guid, 2, 0, 1 },
				new object[] { guid, 3, 0, 1 }
			};

			var count = await _queryFactory.Query("inventory").InsertAsync(columns, data);

			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateUserItemFailInsert }, "CreateUserItemFailInsert");
				return ErrorCode.CreateUserItemFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateUserItemFailException }, "CreateUserItemFailException");
			return ErrorCode.CreateUserItemFailException;
		}
	}

	public async Task<ErrorCode> RollbackUserAsync(byte[] guid)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new {Guid = guid}, "RollbackUser Start");

			var count = await _queryFactory.Query("user_data")
				.Where("AccountId", "=", guid).DeleteAsync();

			if (count < 1)
			{
				_logger.ZLogDebugWithPayload(new {ErrorCode = ErrorCode.RollbackUserDataFailDelete }, "RollbackUserDataFailDelete");
				return ErrorCode.RollbackUserDataFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogDebugWithPayload(new { ErrorCode = ErrorCode.RollbackUserDataFailException }, "RollbackUserDataFailException");
			return ErrorCode.RollbackUserDataFailException;
		}
	}
}