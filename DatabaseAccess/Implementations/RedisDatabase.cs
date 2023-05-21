using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using CloudStructures;
using CloudStructures.Structures;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.Database.Game;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class RedisDatabase : IMemoryDatabase
{
	private RedisConnection _redisConnection;
	private ILogger<RedisDatabase> _logger;
	private ChatRoomAllocator _chatRoomAllocator;

	public RedisDatabase(IOptions<DatabaseConfiguration> configuration, ILogger<RedisDatabase> logger,
		ChatRoomAllocator chatRoomAllocator)
	{
		var config = new RedisConfig("default", configuration.Value.Redis);
		_redisConnection = new RedisConnection(config);
		_logger = logger;
		_chatRoomAllocator = chatRoomAllocator;
	}

	public async Task<ErrorCode> RegisterUserAsync(String email, String authToken, UserData uesrData)
	{
		_logger.ZLogDebugWithPayload(new { Email = email }, "RegisterUser Start");

		var key = MemoryDatabaseKeyGenerator.MakeUIDKey(email);
		var authInfo = new AuthenticatedUserState
		{
			Email = email,
			AuthToken = authToken,
			GameUserId = uesrData.GameUserId,
			PlayerId = uesrData.PlayerId,
			State = UserStateCode.Lobby,
			ChannelNumber = (Int32)_chatRoomAllocator.AllocateRoomNumber()
		};

		try
		{
			var errorCode = await UpdateAuthenticatedUserStateAsync(key, authInfo);
			if (errorCode != ErrorCode.None)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RegisterUserFailSet, Email = email },
					"RegisterUserFailSet");
				return ErrorCode.RegisterUserFailSet;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.RegisterUserFailException, Email = email },
				"RegisterUserFailException");
			return ErrorCode.RegisterUserFailException;
		}
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

	public async Task<(ErrorCode, AuthenticatedUserState)> LoadAuthUserDataAsync(String email)
	{
		var key = MemoryDatabaseKeyGenerator.MakeUIDKey(email);
		_logger.ZLogDebugWithPayload(new { Email = email, Key = key }, "LoadAuthUserData Start");
		try
		{
			var redis = new RedisString<AuthenticatedUserState>(_redisConnection, key, null);
			var userData = await redis.GetAsync();
			if (!userData.HasValue)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadAuthUserDataFailEmpty, Key = key },
					"LoadAuthUserDataFailEmpty");
				return (ErrorCode.LoadAuthUserDataFailEmpty, new AuthenticatedUserState());
			}

			return (ErrorCode.None, userData.Value);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadAuthUserDataFailException, Key = key },
				"LoadAuthUserDataFailException");
			return (ErrorCode.LoadAuthUserDataFailException, new AuthenticatedUserState());
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
			var redis = new RedisString<AuthenticatedUserState>(_redisConnection, key, null);
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

	public async Task<ErrorCode> InsertStageDataAsync(String key, List<KeyValuePair<String, Int32>> stageKeyValueList)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "StoreStageData Start");
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			if (await redis.ExistsAsync() == true)
			{
				var errorCode = await DeleteStageDataAsync(redis, key);
				if (errorCode != ErrorCode.None)
				{
					_logger.ZLogErrorWithPayload(new { Errorcode = ErrorCode.InsertStageDataFailDelete, Key = key },
						"InsertStageDataFailDelete");
					return ErrorCode.InsertStageDataFailDelete;
				}
			}

			await redis.SetAsync(stageKeyValueList);

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.InsertStageDataException, Key = key },
				"InsertStageDataException");
			return ErrorCode.InsertStageDataException;
		}
	}

	public async Task<(ErrorCode, Int32 stageLevel)> LoadStageLevelAsync(String key)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "LoadStageLevel Start");
		var field = MemoryDatabaseKeyGenerator.MakeStageLevelKey();

		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			var stageLevel = await redis.GetAsync(field);
			if (!stageLevel.HasValue)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadStageLevelFailGet, Key = key, Field = field },
					"LoadStageLevelFailGet");
				return (ErrorCode.LoadStageLevelFailGet, 0);
			}

			return (ErrorCode.None, stageLevel.Value);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.LoadStageLevelFailException, Key = key, Field = field },
				"LoadStageLevelFailException");
			return (ErrorCode.LoadStageLevelFailException, 0);
		}
	}

	public async Task<(ErrorCode, Int32 itemAcquisitionCount)> LoadItemAcquisitionCountAsync(String key, Int32 itemCode)
	{
		_logger.ZLogDebugWithPayload(new { Key = key, NpcCode = itemCode }, "LoadItemAcquisitionCount Start");
		var field = MemoryDatabaseKeyGenerator.MakeStageItemKey(itemCode);

		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			var value = await redis.GetAsync(field);

			if (!value.HasValue)
			{
				_logger.ZLogErrorWithPayload(
					new { Errorcode = ErrorCode.LoadItemAcquisitionCountFailGet, Key = key, Field = field },
					"LoadItemAcquisitionCountFailGet");
				return (ErrorCode.LoadItemAcquisitionCountFailGet, 0);
			}

			return (ErrorCode.None, value.Value);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { Errorcode = ErrorCode.LoadItemAcquisitionCountFailException, Key = key, Field = field },
				"LoadItemAcquisitionCountFailException");
			return (ErrorCode.LoadItemAcquisitionCountFailException, 0);
		}
	}

	public async Task<ErrorCode> IncrementItemCountAsync(String key, Int32 itemCode, Int32 ItemCount)
	{
		_logger.ZLogDebugWithPayload(new { Key = key, ItemCode = itemCode, ItemCount = ItemCount },
			"IncrementItemCount Start");

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

	public async Task<(ErrorCode, Int32 npcKillCount)> LoadNpcKillCountAsync(String key, Int32 npcCode)
	{
		_logger.ZLogDebugWithPayload(new { Key = key, NpcCode = npcCode }, "LoadNpcKillCount Start");
		var field = MemoryDatabaseKeyGenerator.MakeStageNpcKey(npcCode);

		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

			var value = await redis.GetAsync(field);

			if (!value.HasValue)
			{
				_logger.ZLogErrorWithPayload(
					new { Errorcode = ErrorCode.LoadNpcKillCountFailGet, Key = key, Field = field },
					"LoadNpcKillCountFailGet");
				return (ErrorCode.LoadNpcKillCountFailGet, 0);
			}

			return (ErrorCode.None, value.Value);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { Errorcode = ErrorCode.LoadNpcKillCountFailException, Key = key, Field = field },
				"LoadNpcKillCountFailException");
			return (ErrorCode.LoadNpcKillCountFailException, 0);
		}
	}

	public async Task<ErrorCode> IncrementNpcKillCountAsync(String key, Int32 npcCode)
	{
		_logger.ZLogDebugWithPayload(new { Key = key, NpcCode = npcCode }, "IncrementNpcKillCount Start");
		var field = MemoryDatabaseKeyGenerator.MakeStageNpcKey(npcCode);
		try
		{
			var redis = new RedisDictionary<String, Int32>(_redisConnection, key, TimeSpan.FromMinutes(15));

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

	public async Task<(ErrorCode, Dictionary<String, Int32>)> LoadStageDataAsync(String key)
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
				return (ErrorCode.LoadStageDataFailGet, new Dictionary<String, Int32>());
			}

			return (ErrorCode.None, dictionary);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.LoadStageDataFailException, Key = key },
				"LoadStageDataFailException");
			return (ErrorCode.LoadStageDataFailException, new Dictionary<String, Int32>());
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
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.DeleteStageDataFailDelete, Key = key },
					"DeleteStageDataFailDelete");
				return ErrorCode.DeleteStageDataFailDelete;
			}

			return errorCode;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.DeleteStageDataFailException, Key = key },
				"DeleteStageDataFailException");
			return ErrorCode.DeleteStageDataFailException;
		}
	}

	public async Task<ErrorCode> UpdateUserStateAsync(String key, AuthenticatedUserState authenticatedUserState,
		UserStateCode stateCode)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "UpdateUserState Start");
		try
		{
			authenticatedUserState.State = stateCode;

			var redis = new RedisString<AuthenticatedUserState>(_redisConnection, key, TimeSpan.FromMinutes(60));
			if (await redis.SetAsync(authenticatedUserState) == false)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateUserStateFailSet, Key = key },
					"UpdateUserStateFailSet");
				return ErrorCode.UpdateUserStateFailSet;
			}
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.UpdateUserStateFailException, Key = key },
				"UpdateUserStateFailException");
			return ErrorCode.UpdateUserStateFailException;
		}


		return ErrorCode.None;
	}

	public async Task<ErrorCode> InsertChatMessageAsync(String key, ChatMessageSent chatMessageSent)
	{
		_logger.ZLogDebugWithPayload(new { Key = key, Email = chatMessageSent.Email }, "InsertChatMessage Start");

		try
		{
			var db = _redisConnection.GetConnection().GetDatabase();

			var result = await db.StreamAddAsync(key, "", JsonSerializer.Serialize(chatMessageSent), maxLength: 50);

			if (result.HasValue == false)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.InsertChatMessageFailInsert, Key = key, Email = chatMessageSent.Email
					},
					"InsertChatMessageFailInsert");
				return ErrorCode.InsertChatMessageFailInsert;
			}

			await db.KeyExpireAsync(key, TimeSpan.FromHours(2));

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.InsertChatMessageFailException, Key = key, Email = chatMessageSent.Email
				},
				"InsertChatMessageFailException");
			return ErrorCode.InsertChatMessageFailException;
		}
	}

	public async Task<(ErrorCode, ChatMessageReceived)> LoadLatestChatMessageAsync(String key, String MessageId)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "LoadLatestChatMessageController Start");

		try
		{
			StreamEntry[]? chatStreamEntries;
			var db = _redisConnection.GetConnection().GetDatabase();
			if (MessageId == "")
			{
				chatStreamEntries = await db.StreamRangeAsync(key, "-", "+", count: 1, messageOrder: Order.Descending);
			}
			else
			{
				chatStreamEntries =
					await db.StreamRangeAsync(key, MessageId, "+", count: 1, messageOrder: Order.Ascending);
			}


			if (!chatStreamEntries.Any())
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadLatestChatMessageFailGet, Key = key },
					"LoadLatestChatMessageFailGet");
				return (ErrorCode.LoadLatestChatMessageFailGet, new ChatMessageReceived());
			}

			var latestChatEntry = chatStreamEntries.FirstOrDefault();
			if (latestChatEntry.IsNull)
			{
				return (ErrorCode.LoadLatestChatMessageFailGet, new ChatMessageReceived());
			}

			ChatMessageSent chatMessage =
				JsonSerializer.Deserialize<ChatMessageSent>(latestChatEntry.Values.First().Value);
			if (chatMessage == null)
			{
				return (ErrorCode.LoadLatestChatMessageFailGet, new ChatMessageReceived());
			}

			return (ErrorCode.None,
				new ChatMessageReceived
					{ MessageId = latestChatEntry.Id, Email = chatMessage.Email, Message = chatMessage.Message });
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadLatestChatMessageFailException, Key = key },
				"LoadLatestChatMessageFailException");
			return (ErrorCode.LoadLatestChatMessageFailException, new ChatMessageReceived());
		}
	}

	public async Task<(ErrorCode, List<ChatMessageReceived>)> LoadLatestChatHistoryAsync(String key, String MessageId)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "LoadLatestChatHistory Start");

		try
		{
			StreamEntry[]? chatStreamEntries;
			var db = _redisConnection.GetConnection().GetDatabase();
			if (MessageId == "")
			{
				chatStreamEntries = await db.StreamRangeAsync(key, "-", "+", count: 50, messageOrder: Order.Descending);
			}
			else
			{
				chatStreamEntries = await db.StreamRangeAsync(key, MessageId, "+", count: 50, messageOrder: Order.Ascending);
			}

			if (!chatStreamEntries.Any())
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadLatestChatMessageFailGet, Key = key },
					"LoadLatestChatMessageFailGet");
				return (ErrorCode.LoadLatestChatMessageFailGet, new List<ChatMessageReceived>());
			}

			List<ChatMessageReceived> chatHistory = new List<ChatMessageReceived>();

			foreach (var chatEntry in chatStreamEntries)
			{
				if (chatEntry.IsNull)
				{
					continue;
				}

				ChatMessageSent chatMessage = JsonSerializer.Deserialize<ChatMessageSent>(chatEntry.Values.First().Value);
				if (chatMessage == null)
				{
					continue;
				}

				chatHistory.Add(
					new ChatMessageReceived
					{
						MessageId = chatEntry.Id,
						Email = chatMessage.Email,
						Message = chatMessage.Message
					}
				);
			}

			return (ErrorCode.None, chatHistory);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadLatestChatMessageFailException, Key = key },
				"LoadLatestChatMessageFailException");
			return (ErrorCode.LoadLatestChatMessageFailException, new List<ChatMessageReceived>());
		}
	}

	public async Task<ErrorCode> UpdateChatChannelAsync(String key, AuthenticatedUserState authenticatedUserState,
		Int32 channelNumber)
	{
		_logger.ZLogDebugWithPayload(new { Key = key, ChannelNumber = channelNumber }, "UpdateChatChannel Start");

		authenticatedUserState.ChannelNumber = channelNumber;

		try
		{
			var errorCode = await UpdateAuthenticatedUserStateAsync(key, authenticatedUserState);

			if (errorCode != ErrorCode.None)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.UpdateChatChannelChatChannelFailUpdate, Key = key,
						ChannelNumber = channelNumber
					},
					"UpdateChatChannelChatChannelFailUpdate");
				return ErrorCode.UpdateChatChannelChatChannelFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.UpdateChatChannelChatChannelFailException, Key = key,
					ChannelNumber = channelNumber
				},
				"UpdateChatChannelChatChannelFailException");
			return ErrorCode.UpdateChatChannelChatChannelFailException;
		}
	}

	private async Task<ErrorCode> UpdateAuthenticatedUserStateAsync(String key,
		AuthenticatedUserState authenticatedUserState)
	{
		_logger.ZLogDebugWithPayload(new { Key = key }, "UpdateAuthenticatedUserState Start");

		try
		{
			var redis = new RedisString<AuthenticatedUserState>(_redisConnection, key, TimeSpan.FromMinutes(60));
			if (await redis.SetAsync(authenticatedUserState) == false)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.UpdateAuthenticatedUserStateFailSet, Key = key },
					"UpdateAuthenticatedUserStateFailSet");
				return ErrorCode.UpdateAuthenticatedUserStateFailSet;
			}
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.UpdateAuthenticatedUserStateFailException, Key = key },
				"UpdateAuthenticatedUserStateFailException");
			return ErrorCode.UpdateAuthenticatedUserStateFailException;
		}


		return ErrorCode.None;
	}

	private async Task<ErrorCode> DeleteStageDataAsync(RedisDictionary<String, Int32> redis, String key)
	{
		if (await redis.DeleteAsync() == false)
		{
			return ErrorCode.DeleteStageDataFail;
		}

		return ErrorCode.None;
	}
}