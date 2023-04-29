
using CloudStructures;
using CloudStructures.Structures;
using DungeonWarAPI.Game;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.ModelDatabase;
using Microsoft.Extensions.Options;
using ZLogger;

namespace DungeonWarAPI.Services;

public class RedisDatabase : IMemoryDatabase
{
	public RedisConnection _redisConnection;
	private ILogger<RedisDatabase> _logger;

	public RedisDatabase(IOptions<DatabaseConfiguration> configuration , ILogger<RedisDatabase> logger)
	{
		var config = new RedisConfig("default", configuration.Value.Redis);
		_redisConnection = new RedisConnection(config);
		_logger = logger;
	}

	public async Task<ErrorCode> RegisterUserAsync(string email, string authToken, UserData uesrData)
	{

		_logger.ZLogDebugWithPayload(new {Email = email}, "RegisterUser Start");
		var authInfo = new AuthInfo
		{
			AuthToken = authToken,
			GameUserId = uesrData.GameUserId,
			PlayerId=uesrData.PlayerId,
		};

		try
		{
			var redis = new RedisString<AuthInfo>(_redisConnection, email, TimeSpan.FromMinutes(60));
			if (await redis.SetAsync(authInfo) == false)
			{
				_logger.ZLogDebugWithPayload(new { ErrorCode = ErrorCode.LoginFailRegisterToRedis, Email = email }, "RegisterUser Fail");
				return ErrorCode.LoginFailRegisterToRedis;
			}
		}
		catch (Exception e)
		{
			_logger.ZLogDebugWithPayload(new { ErrorCode = ErrorCode.LoginFailRegisterToRedisException, Email = email }, "RegisterUser Exception");
			return ErrorCode.LoginFailRegisterToRedisException;
		}


		return ErrorCode.None;
	}

	public async Task<(ErrorCode errorCode, List<string> notifications)> LoadNotificationsAsync()
	{
		try
		{
			var redis = new RedisSet<string>(_redisConnection, "Notifications", null);

			var notifications = await redis.MembersAsync();

			if (!notifications.Any())
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadNotificationsZeroNotification }, "LoadNotificationsZeroNotification");
				return (ErrorCode.None, new List<string> { "공지 없음" });
			}


			return (ErrorCode.None, notifications.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new {ErrorCode = ErrorCode.LoadNotificationsFailException }, "LoadNotificationsException");
			return (ErrorCode.LoadNotificationsFailException, null);
		}
	}
}
