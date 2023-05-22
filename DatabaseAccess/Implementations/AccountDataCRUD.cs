using System.Data;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.DAO.Account;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class AccountDataCRUD : IAccountDataCRUD
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<AccountDataCRUD> _logger;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public AccountDataCRUD(ILogger<AccountDataCRUD> logger, IOptions<DatabaseConfiguration> configurationOptions)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;

		_databaseConnection = new MySqlConnection(configurationOptions.Value.AccountDatabase);
		_databaseConnection.Open();

		var compiler = new MySqlCompiler();
		_queryFactory = new QueryFactory(_databaseConnection, compiler);
	}

	public void Dispose()
	{
		_databaseConnection.Dispose();
		//_queryFactory.Dispose();
	}

	public async Task<(ErrorCode errorCode, Int32 playerId)> CreateAccountAsync(String email, String password)
	{
		_logger.ZLogDebugWithPayload(new { Email = email, Password = password }, $"CreateAccount Start");

		try
		{
			var saltValue = Security.GetNewSalt();
			var hashedPassword = Security.CalcHashedPassword(password, saltValue);

			var accountId = await _queryFactory.Query("account").InsertGetIdAsync<Int32>(new
				{ Email = email, SaltValue = saltValue, HashedPassword = hashedPassword });

			if (accountId <= 0)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateAccountFailInsert, Email = email }, 
					"CreateAccountFailInsert");

				return new(ErrorCode.CreateAccountFailInsert, 0);
			}

			return new(ErrorCode.None, accountId);
		}
		catch (MySqlException e)
		{
			if (e.Number == 1062)
			{
				_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.CreateAccountFailDuplicate, Email = email }, 
					$"CreateAccount Exception");
				
				return (ErrorCode.CreateAccountFailDuplicate, 0);
			}

			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.CreateAccountFailException }, 
				$"CreateAccount Exception");

			return new(ErrorCode.CreateAccountFailException, 0);
		}
	}


	public async Task<ErrorCode> RollbackCreateAccountAsync(Int32 accountId)
	{
		_logger.ZLogDebugWithPayload(new { AccountId = accountId }, $"RollbackCreateAccount Start");
		try
		{
			var count = await _queryFactory.Query("account")
				.Where("AccountId", "=", accountId).DeleteAsync();

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateAccountFailDelete }, 
					"RollbackCreateAccountFailDelete");

				return ErrorCode.RollbackCreateAccountFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackCreateAccountFailException }, 
				"RollbackCreateAccountFailException");
			
			return ErrorCode.RollbackCreateAccountFailException;
		}
	}

	public async Task<(ErrorCode errorCode, Int32 playerId)> VerifyAccount(String email, String password)
	{
		_logger.ZLogDebugWithPayload(new { Email = email }, "VerifyAccount Start");

		try
		{
			var accountInformation = await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();

			if (accountInformation == null)
			{
				_logger.ZLogErrorWithPayload(new { Email = email }, "VerifyAccount Fail");

				return (ErrorCode.LoginFailUserNotExist, 0);
			}

			if (accountInformation.HashedPassword != Security.CalcHashedPassword(password, accountInformation.SaltValue))
			{
				return (ErrorCode.LoginFailWrongPassword, 0);
			}

			return (ErrorCode.None, accountInformation.AccountId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { Email = email }, "VerifyAccount Exception");

			return (ErrorCode.LoginFailException, 0);
		}
	}
}