using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;

namespace firstAPI.Services
{
	public interface IMemoryDatabase
	{
		Task<ErrorCode> RegisterUserAsync(string id, string authToken, Int64 accountID);
		Task<Tuple<ErrorCode, List<string>>> GetNoticeAsync();
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

		public async Task<Tuple<ErrorCode, List<string>>> GetNoticeAsync()
		{

			try
			{
				var redis = new RedisSet<string>(_redisConnection, nameof(Notifications), null);

				var notifications = await redis.MembersAsync();

				if (!notifications.Any())
				{
					return new Tuple<ErrorCode, List<string>>(ErrorCode.None, new List<string> {"공지 없음"});
				}


				return new Tuple<ErrorCode, List<string>>(ErrorCode.None, notifications.ToList());
			}
			catch (Exception e)
			{
				return new Tuple<ErrorCode, List<string>>(ErrorCode.NoticeFailExceptions, null);
			}
			

		}

		
	}

	public class Notifications
	{
		public string Notification { get; set; }
	}

	public class AuthInfo
	{
		public string AuthToken { get; set; }
		public Int64 AccountId { get; set; }

	}
	
}
