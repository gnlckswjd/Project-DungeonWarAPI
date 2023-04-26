using System.Data;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace firstAPI.Services
{
	public interface IAccountDatabase : IDisposable
	{
		public Task<ErrorCode> CreateAccountAsync(String id, String password);
		public Task<Tuple<ErrorCode,Int64>> VerifyAccount(String id, String password);
	}

	public class AccountDatabase : IAccountDatabase
	{
		private readonly IOptions<DatabaseConfiguration> _configurationOptions;
		private readonly ILogger<AccountDatabase> _logger;

		private IDbConnection _databaseConnection;
		QueryFactory _queryFactory;

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

		public async Task<ErrorCode> CreateAccountAsync(String email, String password)
		{
			try
			{
				var saltValue = Security.GetSalt();
				var hashingPassword = Security.GetHashedPassword(password,saltValue);
				Console.WriteLine($"[CreateAccount] Email: {email}, Password: {password}");
				var count = await _queryFactory.Query("account")
					.InsertAsync(new { Email = email, SaltValue = saltValue, HashedPassword = hashingPassword });

				if (count != 1)
				{
					return ErrorCode.CreateAccountFailInsert;
				}

				return ErrorCode.None;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return ErrorCode.CreateAccountFailException;
			}

		}

		public async Task<Tuple<ErrorCode, Int64>> VerifyAccount(string email, string password)
		{

			try
			{
				var accountInformation =
					await _queryFactory.Query("account").Where("Email", email).FirstOrDefaultAsync<Account>();

				if (accountInformation == null || accountInformation.AccountId == 0)
				{
					return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailUserNotExist, 0);
				}

				return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountInformation.AccountId);
			}
			catch (Exception e)
			{
				return new Tuple<ErrorCode, Int64>(ErrorCode.LoginFailException, 0);
			}
			
		}
	}

	public class DatabaseConfiguration
	{
		public String AccountDatabase { get; set; }
		public String GameDb { get; set; }
		public String Redis { get; set; }
	}

	public class Account
	{
		public Int64 AccountId { get; set; }

		public String Email { get; set; }
		public String HashedPassword { get; set; }
		public String SaltValue { get; set; }
	}
}