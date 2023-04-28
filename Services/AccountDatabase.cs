using System.Data;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.ModelDatabase;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.Services;

public class AccountDatabase : IAccountDatabase
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<AccountDatabase> _logger;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public AccountDatabase(ILogger<AccountDatabase> logger, IOptions<DatabaseConfiguration> configurationOptions)
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

	public async Task<ErrorCode> CreateAccountAsync(String email, String password, Byte[] guid)
	{
		try
		{
			var saltValue = Security.GetNewSalt();
			var hashingPassword = Security.GetNewHashedPassword(password, saltValue);


			_logger.ZLogInformationWithPayload( new {Email= email, Password= password} ,$"CreateAccount Start");
			var count = await _queryFactory.Query("account")
				.InsertAsync(new
					{ AccountId = guid, Email = email, SaltValue = saltValue, HashedPassword = hashingPassword });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload( new {ErrorCode=ErrorCode.CreateAccountFailInsert},$"CreateAccount Fail");
				return ErrorCode.CreateAccountFailInsert;
			}

			return ErrorCode.None;
		}
		catch (MySqlException e)
		{
			if (e.Number == 1062)
			{
				_logger.ZLogErrorWithPayload(new {ErrorCode = ErrorCode.CreateAccountFailDuplicate , Email = email},$"CreateAccount Exception");
				return ErrorCode.CreateAccountFailDuplicate;
			}

			_logger.ZLogErrorWithPayload(new {ErrorCode = ErrorCode.CreateAccountFailDuplicate }, $"CreateAccount Exception");
			return ErrorCode.CreateAccountFailException;
		}
	}


	public async Task<ErrorCode> RollbackAccountAsync(Byte[] guid)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new {Guid=guid},$"RollbackAccount Start");
			var count = await _queryFactory.Query("account")
				.Where("AccountId", "=", guid).DeleteAsync();

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new {ErrorCode= ErrorCode.RollbackAccountFailDelete}, "RollbackAccount Fail");
				return ErrorCode.RollbackAccountFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackAccountFailDelete }, "RollbackAccount Exception");
			return ErrorCode.RollbackAccountFailException;
		}
	}

	public async Task<Tuple<ErrorCode, Byte[]>> VerifyAccount(String email, String password)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { Email = email }, "VerifyAccount Start");
			var accountInformation =
				await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();

			if (accountInformation == null)
			{
				_logger.ZLogErrorWithPayload(new { Email = email }, "VerifyAccount Fail");
				return new Tuple<ErrorCode, Byte[]>(ErrorCode.LoginFailUserNotExist, null);
			}

			return new Tuple<ErrorCode, Byte[]>(ErrorCode.None, accountInformation.AccountId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { Email = email }, "VerifyAccount Exception");
			return new Tuple<ErrorCode, Byte[]>(ErrorCode.LoginFailException, null);
		}
	}
}