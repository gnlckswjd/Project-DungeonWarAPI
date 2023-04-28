using System.Data;
using DungeonWarAPI.ModelDatabase;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;

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


			Console.WriteLine($"[CreateAccount] Email: {email}, Password: {password}");
			var count = await _queryFactory.Query("account")
				.InsertAsync(new
					{ AccountId = guid, Email = email, SaltValue = saltValue, HashedPassword = hashingPassword });

			if (count != 1)
			{
				return ErrorCode.CreateAccountFailInsert;
			}

			return ErrorCode.None;
		}
		catch (MySqlException e)
		{
			if (e.Number == 1062)
			{
				return ErrorCode.CreateAccountFailDuplicate;
			}

			Console.WriteLine(e);
			return ErrorCode.CreateAccountFailException;
		}
	}


	public async Task<ErrorCode> RollbackAccountAsync(Byte[] guid)
	{
		try
		{
			var count = await _queryFactory.Query("account")
				.Where("AccountId", "=", guid).DeleteAsync();

			if (count != 1)
			{
				return ErrorCode.RollbackAccountFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return ErrorCode.RollbackAccountFailException;
		}
	}

	public async Task<Tuple<ErrorCode, Byte[]>> VerifyAccount(String email, String password)
	{
		try
		{
			var accountInformation =
				await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();

			if (accountInformation == null)
			{
				return new Tuple<ErrorCode, Byte[]>(ErrorCode.LoginFailUserNotExist, null);
			}

			return new Tuple<ErrorCode, Byte[]>(ErrorCode.None, accountInformation.AccountId);
		}
		catch (Exception e)
		{
			return new Tuple<ErrorCode, Byte[]>(ErrorCode.LoginFailException, null);
		}
	}
}