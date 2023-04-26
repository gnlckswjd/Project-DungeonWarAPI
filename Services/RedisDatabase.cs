using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;

namespace firstAPI.Services
{
	public interface IMemoryDatabase
	{
		Task<ErrorCode> RegisterUserAsync(string id, string authToken, Int64 accountID);
	}
	public class RedisDatabase : IMemoryDatabase
	{
		public RedisConnection _redisConnection;


		public RedisDatabase(IOptions<DatabaseConfiguration> configuration)
		{
			var config = new RedisConfig("default", configuration.Value.Redis);
			_redisConnection = new RedisConnection(config);
		}

		public async Task<ErrorCode> RegisterUserAsync(string email, string authToken, Int64 accountId)
		{

		
			var authInfo = new AuthInfo
			{
				AuthToken = authToken,
				AccountId = accountId
			};

			try
			{
				var redis = new RedisString<AuthInfo>(_redisConnection, email, TimeSpan.FromMinutes(60));
				if (await redis.SetAsync(authInfo) == false)
				{
					return ErrorCode.LoginFailRegisterToRedis;
				}
			}
			catch (Exception e)
			{

				return ErrorCode.LoginFailRegisterToRedisException;
			}
			

			return ErrorCode.None;
		}
	}

	class AuthInfo
	{
		public string AuthToken { get; set; }
		public Int64 AccountId { get; set; }

	}
	
}
