using DungeonWarAPI.ModelConfiguration;
using Humanizer;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services;

public class GameDatabase : IGameDatabase
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<GameDatabase> _logger;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public GameDatabase(ILogger<GameDatabase> logger, IOptions<DatabaseConfiguration> configurationOptions)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;

		_databaseConnection = new MySqlConnection(configurationOptions.Value.GameDatabase);
		_databaseConnection.Open();

		var compiler = new MySqlCompiler();
		_queryFactory = new QueryFactory(_databaseConnection, compiler);
	}


	public void Dispose()
	{
		_databaseConnection.Dispose();
		//_queryFactory.Dispose();
	}

	public async Task<(ErrorCode, Int32 )> CreateUserAsync(int playerId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { PlayerId = playerId }, "CreateUser Start");

			var gameUserId = await _queryFactory.Query("user_data")
				.InsertGetIdAsync<Int32>(new { PlayerId = playerId });

			_logger.ZLogInformationWithPayload(new { GameUserId = gameUserId }, "CreateUser Success");

			return (ErrorCode.None, gameUserId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateUserFailException },
				"CreateUserFailException");
			return (ErrorCode.CreateUserFailException, 0);
		}
	}

	public async Task<ErrorCode> CreateUserItemAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateUserItem Start");
			var columns = new[] { "GameUserId", "ItemCode", "UpgradeLevel", "ItemCount" };
			var data = new[]
			{
				new object[] { gameUserId, 5, 0, 5000 },
				new object[] { gameUserId, 2, 0, 1 },
				new object[] { gameUserId, 3, 0, 1 }
			};

			var count = await _queryFactory.Query("owned_item").InsertAsync(columns, data);

			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.CreateUserItemFailInsert, GameUserId = gameUserId },
					"CreateUserItemFailInsert");
				return ErrorCode.CreateUserItemFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.CreateUserItemFailException, GameUserId = gameUserId },
				"CreateUserItemFailException");
			return ErrorCode.CreateUserItemFailException;
		}
	}

	public async Task<ErrorCode> RollbackCreateUserAsync(Int32 gameUserId)
	{
		try
		{
			_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "RollbackUser Start");

			var count = await _queryFactory.Query("user_data")
				.Where("GameUserId", "=", gameUserId).DeleteAsync();

			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateUserDataFailDelete },
					"RollbackCreateUserDataFailDelete");
				return ErrorCode.RollbackCreateUserDataFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateUserDataFailException },
				"RollbackCreateUserDataFailException");
			return ErrorCode.RollbackCreateUserDataFailException;
		}
	}

	public async Task<(ErrorCode, UserData)> LoadUserData(int playerId)
	{
		_logger.ZLogDebugWithPayload(new { PlayerId = playerId }, "LoadUserData Start");

		try
		{
			var userData = await _queryFactory.Query("user_data")
				.Where("PlayerId", "=", playerId).FirstOrDefaultAsync<UserData>();
			if (userData == null)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadUserDataFailSelect, PlayerId = playerId },
					"ErrorCode.LoadUserDataFailSelect");
				return (ErrorCode.LoadUserDataFailSelect, null);
			}

			return (ErrorCode.None, userData);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadUserDataFailException, PlayerId = playerId },
				"LoadUserDataFailException");
			return (ErrorCode.LoadUserDataFailException, null);
		}
	}

	public async Task<(ErrorCode, List<OwnedItem> )> LoadUserItems(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadUserItems Start");
		try
		{
			var items = await _queryFactory.Query("owned_item").Where("GameUserId", "=", gameUserId)
				.GetAsync<OwnedItem>();

			if (!items.Any())
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadUserItemsFailSelect, GameUserId = gameUserId },
					"LoadUserItemsFailSelect");
				return (ErrorCode.LoadUserItemsFailSelect, null);
			}

			return (ErrorCode.None, items.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.LoadUserItemsFailException, GameUserId = gameUserId },
				"LoadUserItemsFailException");
			return (ErrorCode.LoadUserItemsFailException, null);
		}
	}

	public async Task<(ErrorCode, List<Mail>)> LoadMailList(int gameUserId, int pageNumber)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, PageNumber = pageNumber }, "LoadUserMails Start");

		if (pageNumber < 1)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.LoadMailListWrongPage, GameUserId = gameUserId, PageNumber = pageNumber },
				"LoadMailListWrongPage");
			return (ErrorCode.LoadMailListWrongPage, null);
		}

		try
		{
			var mails = await _queryFactory.Query("mail")
				.Where("GameUserId", "=", gameUserId)
				.OrderByDesc("MailId")
				.Limit(Mail.MailCountInPage).Offset((pageNumber - 1) * Mail.MailCountInPage)
				.GetAsync<Mail>();
			if (!mails.Any())
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.LoadMailListEmptyMail, GameUserId = gameUserId, PageNumber = pageNumber
					},
					"LoadMailListEmptyMail");
				return (ErrorCode.LoadMailListEmptyMail, null);
			}

			return (ErrorCode.None, mails.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new
				{
					ErrorCode = ErrorCode.LoadMailListFailException, GameUserId = gameUserId, PageNumber = pageNumber
				},
				"LoadMailListFailException");
			return (ErrorCode.LoadMailListFailException, null);
		}
	}

	public async Task<ErrorCode> MarkMailAsRead(Int32 gameUserId, Int64 mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId }, "MarkMailAsRead Start");

		try
		{
			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsRead = true });
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.MarkMailAsReadFailUpdate, GameUserId = gameUserId, MailId = mailId },
					"MarkMailAsReadFailUpdate");
				return ErrorCode.MarkMailAsReadFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.MarkMailAsReadFailExceptions, GameUserId = gameUserId, MailId = mailId },
				"MarkMailAsReadFailExceptions");
			return ErrorCode.MarkMailAsReadFailExceptions;
		}
	}

	public async Task<ErrorCode> MarkMailItemAsReceive(Int32 gameUserId, Int64 mailId)
	{
		// 수정 필요

		try
		{
			bool isReceived = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.Select("IsReceived")
				.FirstOrDefaultAsync<bool>();

			if (isReceived == true)
			{
				_logger.ZLogErrorWithPayload(new
				{
					ErrorCode = ErrorCode.MarkMailItemAsReceiveFailAlreadyReceived,
					GameUserId = gameUserId, MailId = mailId
				}, "MarkMailItemAsReceiveFailAlreadyReceived");
				return (ErrorCode.MarkMailItemAsReceiveFailAlreadyReceived);
			}


			//if (isReceived == null)
			//{
			//	_logger.ZLogErrorWithPayload(
			//		new
			//		{
			//			ErrorCode = ErrorCode.MarkMailItemAsReceiveFailSelect, GameUserId = gameUserId, MailId = mailId
			//		},
			//		"MarkMailItemAsReceiveFailSelect");
			//	return (ErrorCode.MarkMailItemAsReceiveFailSelect, null);
			//}

			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsReceived = true });
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.MarkMailItemAsReceiveFailUpdate, GameUserId = gameUserId, MailId = mailId
					}, "MarkMailItemAsReceiveFailUpdate");
				return (ErrorCode.MarkMailItemAsReceiveFailUpdate);
			}

			return (ErrorCode.None);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.MarkMailItemAsReceiveException, GameUserId = gameUserId, MailId = mailId },
				"MarkMailItemAsReceiveException");
			return (ErrorCode.MarkMailItemAsReceiveException);
		}
	}

	public async Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(int gameUserId, long mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId },
			"RollbackMarkMailItemAsReceive Start");
		try
		{
			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsReceived = false });


			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackMarkMailItemAsReceiveFailUpdate,
						GameUserId = gameUserId,
						MailId = mailId
					}, "RollbackMarkMailItemAsReceiveFailUpdate");
				return ErrorCode.RollbackMarkMailItemAsReceiveFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new
				{
					ErrorCode = ErrorCode.RollbackMarkMailItemAsReceiveFailException, GameUserId = gameUserId,
					MailId = mailId
				},
				"RollbackMarkMailItemAsReceiveFailException");
			return ErrorCode.RollbackMarkMailItemAsReceiveFailException;
		}
	}

	public async Task<ErrorCode> ReceiveItemAsync(int gameUserId, Int64 mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId },
			"ReceiveItemAsync Start");
		try
		{
			var items = await _queryFactory.Query("mail_item")
				.Where("MailId", "=", mailId)
				.GetAsync<MailItem>();

			if (!items.Any())
			{
				_logger.ZLogErrorWithPayload(new
					{
						ErrorCode = ErrorCode.ReceiveItemFailMailHasNotItem, GameUserId = gameUserId, MailId = mailId
					}
					, "ReceiveItemFailMailHasNotItem");
				return ErrorCode.ReceiveItemFailMailHasNotItem;
			}

			var columns = new[] { "GameUserId", "ItemCode", "EnhancedCount", "ItemCount" };
			var data = items.Select(item => new object[]
				{ gameUserId, item.ItemCode, item.EnhancedCount, item.ItemCount }).ToList();

			var count = await _queryFactory.Query("owned_item").InsertAsync(columns, data);

			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(new
						{ ErrorCode = ErrorCode.ReceiveItemFailInsert, GameUserId = gameUserId, MailId = mailId }
					, "ReceiveItemFailInsert");
				return ErrorCode.ReceiveItemFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new
					{ ErrorCode = ErrorCode.ReceiveItemFailException, GameUserId = gameUserId, MailId = mailId }
				, "ReceiveItemFailException");
			return ErrorCode.ReceiveItemFailException;
		}
	}

	public async Task<ErrorCode> VerifyMailOwnerId(Int32 gameUserId, Int64 mailId)
	{
		try
		{
			var mailUserId = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.Select("GameUserId")
				.FirstOrDefaultAsync<Int32>();

			if (mailUserId != gameUserId)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.VerifyMailOwnerIdFailWrongId, GameUserId = gameUserId, MailId = mailId
					}, "VerifyMailOwnerIdFailWrongId");
				return ErrorCode.VerifyMailOwnerIdFailWrongId;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.VerifyMailOwnerIdFailException, GameUserId = gameUserId, MailId = mailId },
				"VerifyMailOwnerIdFailException");
			return ErrorCode.VerifyMailOwnerIdFailException;
		}
	}
}