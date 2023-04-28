using DungeonWarAPI.ModelDatabase;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;

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
			Console.WriteLine($"[Create UserData] guid: {guid}");

			var count = await _queryFactory.Query("user_data")
				.InsertAsync(new { AccountId = guid });

			if (count != 1)
			{
				return ErrorCode.CreateUserFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return ErrorCode.CreateUserFailException;
		}
	}

	public async Task<ErrorCode> CreateUserItemAsync(Byte[] guid)
	{
		try
		{
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
				return ErrorCode.CreateUserItemFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return ErrorCode.CreateUserItemFailException;
		}
	}

	public async Task<ErrorCode> RollbackUserAsync(byte[] guid)
	{
		try
		{
			var count = await _queryFactory.Query("user_data")
				.Where("AccountId", "=", guid).DeleteAsync();

			if (count < 1)
			{
				return ErrorCode.RollbackUserDataFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return ErrorCode.RollbackUserDataFailException;
		}
	}
}