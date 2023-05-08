using DungeonWarAPI.Managers;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Models.Database.Game;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class MailService : IMailService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<MailService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public MailService(ILogger<MailService> logger, IOptions<DatabaseConfiguration> configurationOptions,
		MasterDataManager masterData)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;
		_masterData = masterData;

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
	public async Task<(ErrorCode, List<MailWithItems>)> LoadMailListAsync(int gameUserId, int pageNumber)
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
				.Where("ExpirationDate", ">", DateTime.Today)
				.Where("isRemoved", "=", false)
				.OrderByDesc("MailId")
				.Limit(Mail.MailCountInPage).Offset((pageNumber - 1) * Mail.MailCountInPage)
				.GetAsync<Mail>();

			if (!mails.Any())
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.LoadMailListEmptyMail,
						GameUserId = gameUserId,
						PageNumber = pageNumber
					},
					"LoadMailListEmptyMail");
				return (ErrorCode.LoadMailListEmptyMail, null);
			}

			var mailsWithItems = new List<MailWithItems>();

			foreach (var mail in mails)
			{
				var (errorCode, items) = await GetMailItemsAsync(gameUserId, mail.MailId);

				if (errorCode != ErrorCode.None)
				{
					return (errorCode, null);
				}

				mailsWithItems.Add(new MailWithItems { Mail = mail, Items = items });
			}


			return (ErrorCode.None, mailsWithItems.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.LoadMailListFailException,
					GameUserId = gameUserId,
					PageNumber = pageNumber
				},
				"LoadMailListFailException");
			return (ErrorCode.LoadMailListFailException, null);
		}
	}

	public async Task<ErrorCode> MarkMailAsReadAsync(int gameUserId, long mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId }, "MarkMailAsRead Start");

		try
		{
			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				//.Where("GameUserId","=",gameUserId)
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
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.MarkMailAsReadFailExceptions, GameUserId = gameUserId, MailId = mailId },
				"MarkMailAsReadFailExceptions");
			return ErrorCode.MarkMailAsReadFailExceptions;
		}
	}

	public async Task<ErrorCode> MarkMailItemAsReceiveAsync(int gameUserId, long mailId)
	{
		try
		{
			bool isReceived = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.Where("GameUserId", "=", gameUserId)
				.Select("IsReceived")
				.FirstOrDefaultAsync<bool>();

			if (isReceived == true)
			{
				_logger.ZLogErrorWithPayload(new
				{
					ErrorCode = ErrorCode.MarkMailItemAsReceiveFailAlreadyReceived,
					GameUserId = gameUserId,
					MailId = mailId
				}, "MarkMailItemAsReceiveFailAlreadyReceived");
				return ErrorCode.MarkMailItemAsReceiveFailAlreadyReceived;
			}


			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsReceived = true });
			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.MarkMailItemAsReceiveFailUpdate,
						GameUserId = gameUserId,
						MailId = mailId
					}, "MarkMailItemAsReceiveFailUpdate");
				return ErrorCode.MarkMailItemAsReceiveFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.MarkMailItemAsReceiveException, GameUserId = gameUserId, MailId = mailId },
				"MarkMailItemAsReceiveException");
			return ErrorCode.MarkMailItemAsReceiveException;
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
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.RollbackMarkMailItemAsReceiveFailException,
					GameUserId = gameUserId,
					MailId = mailId
				},
				"RollbackMarkMailItemAsReceiveFailException");
			return ErrorCode.RollbackMarkMailItemAsReceiveFailException;
		}
	}

	public async Task<ErrorCode> ReceiveItemAsync(int gameUserId, long mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId },
			"ReceiveItemAsync Start");

		var (errorCode, items) = await GetMailItemsAsync(gameUserId, mailId);

		if (items.Count() < 1)
		{
			_logger.ZLogErrorWithPayload(new
			{
				ErrorCode = ErrorCode.ReceiveItemFailMailHaveNoItem,
				GameUserId = gameUserId,
				MailId = mailId
			}
				, "ReceiveItemFailMailHaveNoItem");
			return ErrorCode.ReceiveItemFailMailHaveNoItem;
		}


		List<Func<Task>> rollbackActions = new List<Func<Task>>();

		try
		{
			foreach (var item in items)
			{
				if (item.ItemCode == 1)
				{
					errorCode = await IncreaseGoldAsync(gameUserId, item.ItemCount, rollbackActions);
				}
				else if (item.ItemCode == 6)
				{
					errorCode = await IncreasePotionAsync(gameUserId, item.ItemCount, rollbackActions);
				}
				else
				{
					errorCode = await InsertOwnedItemAsync(gameUserId, item.ItemCode, item.ItemCount,
						item.EnhancementCount,
						rollbackActions);
				}

				if (errorCode != ErrorCode.None)
				{
					await RollbackReceiveItemAsync(rollbackActions);
					_logger.ZLogErrorWithPayload(new
					{ ErrorCode = ErrorCode.ReceiveItemFailInsert, GameUserId = gameUserId, MailId = mailId }
						, "ReceiveItemFailInsert");
					return ErrorCode.ReceiveItemFailInsert;
				}
			}


			return ErrorCode.None;
		}
		catch (Exception e)
		{
			await RollbackReceiveItemAsync(rollbackActions);
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.ReceiveItemFailException, GameUserId = gameUserId, MailId = mailId }
				, "ReceiveItemFailException");
			return ErrorCode.ReceiveItemFailException;
		}
	}

	public async Task<ErrorCode> DeleteMailAsync(int gameUserId, long mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId }, "DeleteMail Start");


		try
		{
			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.Where("GameUserId", "=", gameUserId)
				.Where("IsReceived", "=", true)
				.UpdateAsync(new { IsRemoved = true });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.DeleteMailFailDelete, GameUserId = gameUserId, MailId = mailId },
					"DeleteMailFailDelete");
				return ErrorCode.DeleteMailFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.DeleteMailFailException, GameUserId = gameUserId, MailId = mailId },
				"DeleteMailFailException");
			return ErrorCode.DeleteMailFailException;
		}
	}


	private async Task<(ErrorCode errorCode, List<MailItem> items)> GetMailItemsAsync(int gameUserId, long mailId)
	{
		try
		{
			var items = await _queryFactory.Query("mail_item")
				.Where("MailId", "=", mailId)
				.GetAsync<MailItem>();

			if (items.Count() < 1)
			{
				return (ErrorCode.None, new List<MailItem>());
			}

			return (ErrorCode.None, items.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new
				{
					ErrorCode = ErrorCode.GetMailItemsFailException,
					GameUserId = gameUserId,
					MailId = mailId
				}
				, "GetMailItemsFailException");
			return (ErrorCode.GetMailItemsFailException, null);
		}
	}
	private async Task<ErrorCode> IncreaseGoldAsync(int gameUserId, int itemCount, List<Func<Task>> rollbackActions)
	{
		var count = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
			.IncrementAsync("Gold", itemCount);

		if (count != 1)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.IncreaseGoldFailUpdate, GameUserId = gameUserId },
				"IncreaseGoldFailUpdate");
			return ErrorCode.IncreaseGoldFailUpdate;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.DecrementAsync("Gold", itemCount);
			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackIncreaseGoldFail,
						GameUserId = gameUserId,
						ItemCount = itemCount
					}, "RollbackIncreaseGoldFail");
			}
		});
		return ErrorCode.None;
	}

	private async Task<ErrorCode> IncreasePotionAsync(int gameUserId, int itemCount,
		List<Func<Task>> rollbackActions)
	{
		ErrorCode errorCode = ErrorCode.None;
		var count = await _queryFactory.Query("owned_item").Where("GameUserId", "=", gameUserId)
			.Where("ItemCode", "=", 6)
			.IncrementAsync("ItemCount", itemCount);
		if (count == 0)
		{
			errorCode = await InsertOwnedItemAsync(gameUserId, 6, itemCount,
				0, rollbackActions);
		}

		if (errorCode != ErrorCode.None)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.IncreasePotionFailUpdateOrInsert, GameUserId = gameUserId },
				"IncreasePotionFailUpdateOrInsert");
			return ErrorCode.IncreasePotionFailUpdateOrInsert;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("owned_item").Where("GameUserId", "=", gameUserId)
				.DecrementAsync("ItemCount", itemCount);

			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackIncreasePotionFail,
						GameUserId = gameUserId,
						ItemCount = itemCount
					}, "RollbackIncreasePotionFail");
			}
		});
		return ErrorCode.None;
	}

	private async Task<ErrorCode> InsertOwnedItemAsync(int gameUserId, int itemCode, int itemCount,
		int enhancementCount, List<Func<Task>> rollbackActions)
	{
		var itemId = await _queryFactory.Query("owned_item").InsertGetIdAsync<int>(new
		{
			GameUserId = gameUserId,
			ItemCode = itemCode,
			ItemCount = itemCount,
			EnhancementCount = enhancementCount
		});

		if (itemId == 0)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.InsertOwnedItemFailInsert, GameUserId = gameUserId },
				"InsertOwnedItemFailInsert");
			return ErrorCode.InsertOwnedItemFailInsert;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("user_data").Where("ItemId", "=", itemId)
					.DeleteAsync()
				;
			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackInsertOwnedItemFail,
						GameUserId = gameUserId,
						ItemId = itemId
					}, "RollbackInsertOwnedItemFail");
			}
		});

		return ErrorCode.None;
	}

	private async Task RollbackReceiveItemAsync(List<Func<Task>> rollbackActions)
	{
		foreach (var action in rollbackActions)
		{
			await action();
		}
	}
	
}