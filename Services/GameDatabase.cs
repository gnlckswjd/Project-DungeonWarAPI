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

	public async Task<ErrorCode> CreateUserAsync(byte[] guid)
	{
		try
		{
			Console.WriteLine($"[Create UserData] guid: {guid}");

			var count = await _queryFactory.Query("user_data")
				.InsertAsync( new {AccountId=guid});

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


}