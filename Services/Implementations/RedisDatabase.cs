using CloudStructures;
using CloudStructures.Structures;
using Dapper;
using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class RedisDatabase : IMemoryDatabase
{
	public RedisConnection _redisConnection;
	private ILogger<RedisDatabase> _logger;

	public RedisDatabase(IOptions<DatabaseConfiguration> configuration, ILogger<RedisDatabase> logger)
	{
		var config = new RedisConfig("default", configuration.Value.Redis);
		_redisConnection = new RedisConnection(config);
		_logger = logger;
	}

	public async Task<ErrorCode> RegisterUserAsync(String email, String authToken, UserData uesrData)
	{
		_logger.ZLogDebugWithPayload(new { Email = email }, "RegisterUser Start");

		var key = MemoryDatabaseKeyGenerator.MakeUIDKey(email);
		var authInfo = new AuthUserData
		{
			Email = email,
			AuthToken = authToken,
			GameUserId = uesrData.GameUserId,
			PlayerId = uesrData.PlayerId,
		};

		try
		{
			var redis = new RedisString<AuthUserData>(_redisConnection, key, TimeSpan.FromMinutes(60));
			if (await redis.SetAsync(authInfo) == false)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RegisterUserFailSet, Email = email },
					"RegisterUserFailSet");
				return ErrorCode.RegisterUserFailSet;
			}
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.RegisterUserFailException, Email = email },
				"RegisterUserFailException");
			return ErrorCode.RegisterUserFailException;
		}


		return ErrorCode.None;
	}

	public async Task<(ErrorCode, List<String>)> LoadNotificationsAsync()
	{
		_logger.ZLogDebugWithPayload(new { }, "LoadNotifications Start");

		try
		{
			var redis = new RedisSet<String>(_redisConnection, "Notifications", null);

			var notifications = await redis.MembersAsync();

			if (!notifications.Any())
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadNotificationsZeroNotification },
					"LoadNotificationsZeroNotification");
				return (ErrorCode.None, new List<String> { "공지 없음" });
			}


			return (ErrorCode.None, notifications.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadNotificationsFailException },
				"LoadNotificationsException");
			return (ErrorCode.LoadNotificationsFailException, new List<string>());
		}
	}

	public async Task<(ErrorCode, AuthUserData)> LoadAuthUserDataAsync(String email)
	{
		var key = MemoryDatabaseKeyGenerator.MakeUIDKey(email);
		_logger.ZLogDebugWithPayload(new { Email = email, Key = key }, "LoadAuthUserData Start");
		try
		{
			var redis = new RedisString<AuthUserData>(_redisConnection, key, null);
			var userData = await redis.GetAsync();
			if (!userData.HasValue)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadAuthUserDataFailEmpty, Key = key },
					"LoadAuthUserDataFailEmpty");
				return (ErrorCode.LoadAuthUserDataFailEmpty, new AuthUserData());
			}

			return (ErrorCode.None, userData.Value);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadAuthUserDataFailException, Key = key },
				"LoadAuthUserDataFailException");
			return (ErrorCode.LoadAuthUserDataFailException, new AuthUserData());
		}
	}

	public async Task<ErrorCode> LockUserRequestAsync(String key, String authToken)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "SetUserRequestLock Start");
		try
		{
			var redis = new RedisString<String>(_redisConnection, key, TimeSpan.FromSeconds(3));

			if (await redis.SetAsync(authToken, TimeSpan.FromSeconds(3),
				    StackExchange.Redis.When.NotExists) == false)
			{
				_logger.ZLogErrorWithPayload(new { ErrroCode = ErrorCode.LockUserRequestFailISet },
					"LockUserRequestFailISet");
				return ErrorCode.LockUserRequestFailISet;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrroCode = ErrorCode.LockUserRequestFailExceptions },
				"LockUserRequestFailExceptions");
			return ErrorCode.LockUserRequestFailExceptions;
		}
	}

	public async Task<ErrorCode> UnLockUserRequestAsync(String key)
	{
		_logger.ZLogDebugWithPayload(new { }, "UnLockUserRequestAsync Start");

		if (String.IsNullOrEmpty(key))
		{
			return ErrorCode.UnLockUserRequestFailNullKey;
		}

		try
		{
			var redis = new RedisString<AuthUserData>(_redisConnection, key, null);
			if (await redis.DeleteAsync() == false)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UnLockUserRequestFailDelete, Key = key },
					"UnLockUserRequestFailDelete");
				return ErrorCode.UnLockUserRequestFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.UnLockUserRequestFailException, Key = key },
				"UnLockUserRequestFailExceptions");
			return ErrorCode.UnLockUserRequestFailException;
		}
	}

	public async Task<ErrorCode> InitializeStageDataAsync(String key, List<StageItem> items, List<StageNpc> npcs,
		Int32 stageLevel)
	{
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			if (await redis.ExistsAsync() == true)
			{
				var errorCode = await DeleteStageDataAsync(redis, key);
				if (errorCode != ErrorCode.None)
				{
					_logger.ZLogErrorWithPayload(new { Errorcode = ErrorCode.InitializeStageDataFailDelete, Key = key },
						"InitializeStageDataFailDelete");
					return ErrorCode.InitializeStageDataFailDelete;
				}
			}

			var itemKeys = items.Select(item => MemoryDatabaseKeyGenerator.MakeStageItemKey(item.ItemCode));

			var npcKeys = npcs.Select(npc => MemoryDatabaseKeyGenerator.MakeStageNpcKey(npc.NpcCode));

			var list = itemKeys.Concat(npcKeys)
				.Select(key => new KeyValuePair<String, Int32>(key, 0))
				.ToList();

			list.Add(
				new KeyValuePair<String, Int32>(MemoryDatabaseKeyGenerator.MakeStageLevelKey(), stageLevel));
			await redis.SetAsync(list);

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,new { ErrorCode = ErrorCode.InitializeStageDataFailException, Key = key },
				"InitializeStageDataFailException");
			return ErrorCode.InitializeStageDataFailException;
		}
	}

	public async Task<ErrorCode> IncrementItemCountAsync(String key, Int32 itemCode, Int32 ItemCount)
	{
		var field = MemoryDatabaseKeyGenerator.MakeStageItemKey(itemCode);
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));


			if (await redis.ExistsAsync(field) == false)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.IncrementItemFailNoExist, Key = key, Field = field },
					"IncrementItemFailNoExist");
				return ErrorCode.IncrementItemFailNoExist;
			}

			if (itemCode != (Int32)ItemCode.Gold && itemCode != (Int32)ItemCode.Potion)
			{
				ItemCount = 1;
			}

			var value = await redis.IncrementAsync(field, ItemCount);

			if (value == 0)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.IncrementItemFailIncrease, Key = key, Field = field },
					"IncrementItemFailIncrease");
				return ErrorCode.IncrementItemFailIncrease;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.IncrementItemFailException, Key = key, Field = field },
				"IncrementItemFailException");
			return ErrorCode.IncrementItemFailException;
		}
	}

	public async Task<ErrorCode> IncrementNpcKillCountAsync(String key, Int32 npcCode)
	{
		var field = MemoryDatabaseKeyGenerator.MakeStageNpcKey(npcCode);
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));


			if (await redis.ExistsAsync(field) == false)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.IncrementNpcKillCountFailNoExist, Key = key, Field = field },
					"IncrementNpcKillCountFailNoExist");
				return ErrorCode.IncrementNpcKillCountFailNoExist;
			}

			var value = await redis.IncrementAsync(field, 1);

			if (value == 0)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.IncrementNpcKillCountFailIncrease, Key = key, Field = field },
					"IncrementNpcKillCountFailIncrease");
				return ErrorCode.IncrementNpcKillCountFailIncrease;
			}


			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { e, ErrorCode = ErrorCode.IncrementNpcKillCountFailException, Key = key, Field = field },
				"IncrementNpcKillCountFailException");
			return ErrorCode.IncrementNpcKillCountFailException;
		}
	}

	public async Task<(ErrorCode,Dictionary<String,Int32>)> LoadStageDataAsync(String key)
	{
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			var dictionary = await redis.GetAllAsync();

			if (!dictionary.Any())
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadStageDataFailGet, Key = key },
					"LoadStageDataFailGet");
				return (ErrorCode.LoadStageDataFailGet, new Dictionary<string, int>());
			}

			return (ErrorCode.None, dictionary);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.LoadStageDataFailException, Key = key },
				"LoadStageDataFailException");
			return (ErrorCode.LoadStageDataFailException, new Dictionary<string, int>());
		}

	}


	public async Task<ErrorCode> StoreUserMailPageAsync(AuthUserData authUserData, Int32 pageNumber)
	{
		_logger.ZLogDebugWithPayload(new { authUserData.GameUserId, PageNumber = pageNumber },
			"StoreUserMailPage Start");
		var key = MemoryDatabaseKeyGenerator.MakeMailPageKey(authUserData.Email);
		try
		{
			var redis = new RedisString<Int32>(_redisConnection, key, TimeSpan.FromHours(2));

			if (await redis.SetAsync(pageNumber) == false)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.StoreUserMailPageFailSet, authUserData.GameUserId },
					"StoreUserMailPageFailSet");
				return ErrorCode.StoreUserMailPageFailSet;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.StoreUserMailPageFailException, authUserData.GameUserId },
				"StoreUserMailPageFailException");
			return ErrorCode.StoreUserMailPageFailException;
		}
	}

	public async Task<ErrorCode> DeleteStageDataAsync(String key)
	{
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			var errorCode = await DeleteStageDataAsync(redis, key);
			if (errorCode != ErrorCode.None)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.DeleteStageDataFailDelete, Key = key }, "DeleteStageDataFailDelete");
				return ErrorCode.DeleteStageDataFailDelete;
			}
			return errorCode;
		}
		catch (Exception e)
		{
			
			_logger.ZLogErrorWithPayload(new{ ErrorCode= ErrorCode.DeleteStageDataFailException, Key = key}, "DeleteStageDataFailException");
			return ErrorCode.DeleteStageDataFailException;
		}
		
	}
	private async Task<ErrorCode> DeleteStageDataAsync(RedisDictionary<String, Int32> redis, String key)
	{
		if (await redis.DeleteAsync() == false)
		{
			return ErrorCode.StageDataDeleteFail;
		}

		return ErrorCode.None;
	}
}