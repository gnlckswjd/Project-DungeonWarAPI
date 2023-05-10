using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class DungeonStageService : IDungeonStageService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<DungeonStageService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public DungeonStageService(ILogger<DungeonStageService> logger,
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

	public async Task<(ErrorCode, Int32)> LoadStageList(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadStageList Start");

		try
		{
			var userStage = await _queryFactory.Query("user_stage").Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserStage>();

			if (userStage == null)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadUserStageFailSelect, GameUserId = gameUserId },
					"LoadUserStageFailSelect");
				return (ErrorCode.LoadUserStageFailSelect, 0);
			}

			return (ErrorCode.None, userStage.MaxClearedStage);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadUserStageFailSelect, GameUserId = gameUserId },
				"LoadUserStageFailException");
			return (ErrorCode.LoadUserStageFailException, 0);
		}
	}
}