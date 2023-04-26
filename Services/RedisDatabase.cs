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
			var tempKey = "RegisterUser";
			
			var result = ErrorCode.None;
			try
			{
				var redis = new RedisString<string>(_redisConnection, tempKey, TimeSpan.FromMinutes(15));

				await redis.SetAsync(authToken);
			}
			catch (Exception e)
			{

				result = ErrorCode.LoginFailRegisterToRedis;
				return result;
			}
			

			return result;
		}
	}
}
