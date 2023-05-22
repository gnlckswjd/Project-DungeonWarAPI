using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.Database.Game;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class MailDataCRUD : DatabaseAccessBase, IMailDataCRUD
{
	public MailDataCRUD(ILogger<MailDataCRUD> logger, QueryFactory queryFactory)
		: base(logger, queryFactory)
	{
	}

	public async Task<(ErrorCode, List<Mail>)> LoadMailListAsync(Int32 gameUserId, Int32 pageNumber)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, PageNumber = pageNumber }, "LoadUserMails Start");

		if (pageNumber < 1)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadMailListWrongPage, GameUserId = gameUserId, PageNumber = pageNumber }, 
				"LoadMailListWrongPage");

			return (ErrorCode.LoadMailListWrongPage, new List<Mail>());
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
				_logger.ZLogInformationWithPayload(new { ErrorCode = ErrorCode.LoadMailListEmptyMail, GameUserId = gameUserId, PageNumber = pageNumber },
					"LoadMailListEmptyMail");

				return (ErrorCode.None, new List<Mail>());
			}

			return (ErrorCode.None, mails.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadMailListFailException, GameUserId = gameUserId, PageNumber = pageNumber }, 
				"LoadMailListFailException");

			return (ErrorCode.LoadMailListFailException, new List<Mail>());
		}
	}

	public async Task<(ErrorCode, String content)> ReadMailAsync(Int32 gameUserId, Int64 mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId }, "LoadMail Start");

		try
		{
			var (errorCode, content) = await SelectMailAsync(gameUserId, mailId);
			if (errorCode != ErrorCode.None)
			{
				return (errorCode, "");
			}

			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsRead = true });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.ReadMailFailUpdate, GameUserId = gameUserId, MailId = mailId }, 
					"ReadMailFailUpdate");

				return (ErrorCode.ReadMailFailUpdate, "");
			}

			return (ErrorCode.None, content);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.ReadMailFailExceptions, GameUserId = gameUserId, MailId = mailId }, 
				"ReadMailFailExceptions");

			return (ErrorCode.ReadMailFailExceptions, "");
		}
	}

	public async Task<ErrorCode> UpdateMailStatusToReceivedAsync(Int32 gameUserId, Int64 mailId)
	{
		try
		{
			var mail = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.FirstOrDefaultAsync<Mail>();

			var errorCode = VerifyUserAndCheckIsReceived(gameUserId, mail.GameUserId, mailId, mail.IsReceived);
			if (errorCode != ErrorCode.None)
			{
				return errorCode;
			}

			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsReceived = true });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateMailStatusToReceivedUpdate, GameUserId = gameUserId, MailId = mailId },
					"UpdateMailStatusToReceivedUpdate");

				return ErrorCode.UpdateMailStatusToReceivedUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.UpdateMailStatusToReceivedException, GameUserId = gameUserId, MailId = mailId },
				"UpdateMailStatusToReceivedException");

			return ErrorCode.UpdateMailStatusToReceivedException;
		}
	}

	public async Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, MailId = mailId }, "RollbackMarkMailItemAsReceive Start");

		try
		{
			var count = await _queryFactory.Query("mail")
				.Where("MailId", "=", mailId)
				.UpdateAsync(new { IsReceived = false });


			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackMarkMailItemAsReceiveFailUpdate, GameUserId = gameUserId, MailId = mailId },
					"RollbackMarkMailItemAsReceiveFailUpdate");

				return ErrorCode.RollbackMarkMailItemAsReceiveFailUpdate;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackMarkMailItemAsReceiveFailException, GameUserId = gameUserId, MailId = mailId },
				"RollbackMarkMailItemAsReceiveFailException");

			return ErrorCode.RollbackMarkMailItemAsReceiveFailException;
		}
	}


	public async Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId)
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
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.DeleteMailFailDelete, GameUserId = gameUserId, MailId = mailId }, 
					"DeleteMailFailDelete");

				return ErrorCode.DeleteMailFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.DeleteMailFailException, GameUserId = gameUserId, MailId = mailId },
				"DeleteMailFailException");

			return ErrorCode.DeleteMailFailException;
		}
	}


	public async Task<ErrorCode> CreateInAppMailAsync(Int32 gameUserId, List<PackageItem> packageItems)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateInAppMail Start");

		long mailId = 0;
		try
		{
			mailId = await _queryFactory.Query("mail")
				.InsertGetIdAsync<Int64>(MailGenerator.CreateInAppMail(gameUserId, packageItems[0].PackageId));//PackageId는 packageItems의 모든 요소가 동일한 값을 가집니다.

			if (mailId < 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateInAppMailFailInsertMail, GameUserId = gameUserId, MailId = mailId }, 
					"CreateInAppMailFailInsertMail");

				return ErrorCode.CreateInAppMailFailInsertMail;
			}

			var errorCode = await CreateMailItemsAsync(gameUserId, mailId, packageItems);
			if (errorCode != ErrorCode.None)
			{
				await RollbackCreateMailAsync(mailId);
				return errorCode;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.CreateInAppMailFailException, GameUserId = gameUserId, MailId = mailId },
				"CreateInAppMailFailException");

			await RollbackCreateMailAsync(mailId);
			return ErrorCode.CreateInAppMailFailException;
		}
	}


	public async Task<(ErrorCode, Int64 mailId)> InsertMailAsync(Int32 gameUserId, Mail mail)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "InsertMail Start");
		Int64 mailId = 0;

		try
		{
			mailId = await _queryFactory.Query("mail")
				.InsertGetIdAsync<Int64>(mail);

			if (mailId < 1)
			{
				_logger.ZLogErrorWithPayload( new { ErrorCode = ErrorCode.InsertMailFailInsert, GameUserId = gameUserId, MailId = mailId },
					"InsertMailFailInsert");

				return (ErrorCode.InsertMailFailInsert, 0);
			}

			return (ErrorCode.None, mailId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.InsertMailFailException, GameUserId = gameUserId, MailId = mailId },
				"InsertMailFailException");

			return (ErrorCode.InsertMailFailException, 0);
		}
	}

	public async Task<ErrorCode> RollbackInsertMailAsync(Int64 mailId)
	{
		_logger.ZLogDebugWithPayload( new { MailId = mailId }, "RollbackInsertMail Start");
		
		if (mailId == 0)
		{
			_logger.ZLogErrorWithPayload( new { ErrorCode = ErrorCode.RollbackInsertMailFailWrongMailId, MailId = mailId }, 
				"RollbackInsertMail Start");

			return ErrorCode.RollbackInsertMailFailWrongMailId;
		}

		try
		{
			var count = await _queryFactory.Query("mail").Where("MailId", "=", mailId).DeleteAsync();

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackInsertMailFailDelete, MailId = mailId }, 
					"RollbackInsertMailFailDelete");

				return ErrorCode.RollbackInsertMailFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackInsertMailFailException, MailId = mailId }, 
				"RollbackInsertMailFailException");

			return ErrorCode.RollbackInsertMailFailException;
		}
	}

	public async Task<ErrorCode> InsertMailItemAsync(Int64 mailId, Int32 itemCode, Int32 itemCount)
	{
		_logger.ZLogDebugWithPayload(new { MailId = mailId, ItemCode = itemCode }, "");

		try
		{
			var count = await _queryFactory.Query("mail_item")
				.InsertAsync(new { MailId = mailId, ItemCode = itemCode, ItemCount = itemCount, });

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { Errorcode = ErrorCode.InsertMailItemFailInsert, MailId = mailId, ItemCode = itemCode, itemCount }, 
					"InsertMailItemFailInsert");

				return ErrorCode.InsertMailItemFailInsert;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { Errorcode = ErrorCode.InsertMailItemFailException, MailId = mailId, ItemCode = itemCode, itemCount },
				"InsertMailItemFailException");

			return ErrorCode.InsertMailItemFailException;
		}
	}

	private async Task<(ErrorCode, String)> SelectMailAsync(Int32 gameUserId, Int64 mailId)
	{
		var mail = await _queryFactory.Query("mail")
			.Where("MailId", "=", mailId)
			.FirstOrDefaultAsync<Mail>();

		if (mail == null)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.ReadMailFailSelect, GameUserId = gameUserId, MailId = mailId },
				"ReadMailFailSelect");

			return (ErrorCode.ReadMailFailSelect, "");
		}

		if (mail.GameUserId != gameUserId)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.ReadMailFailWrongUser, GameUserId = gameUserId, MailId = mailId }, 
				"ReadMailFailWrongUser");

			return (ErrorCode.ReadMailFailWrongUser, "");
		}

		return (ErrorCode.None, mail.Contents);
	}


	public async Task<(ErrorCode errorCode, List<MailItem> items)> LoadMailItemsAsync(Int32 gameUserId, Int64 mailId)
	{
		try
		{
			var items = await _queryFactory.Query("mail_item")
				.Where("MailId", "=", mailId)
				.GetAsync<MailItem>();

			if (!items.Any())
			{
				return (ErrorCode.None, new List<MailItem>());
			}

			return (ErrorCode.None, items.ToList());
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.LoadMailItemsFailException, GameUserId = gameUserId, MailId = mailId }, 
				"LoadMailItemsFailException");

			return (ErrorCode.LoadMailItemsFailException, new List<MailItem>());
		}
	}

	private async Task<ErrorCode> CreateMailItemsAsync(Int32 gameUserId, Int64 mailId, List<PackageItem>packageItems)
	{
		try
		{
			var mailItemsData = packageItems.Select(item => new object[] { mailId, item.ItemCode, item.ItemCount }).ToArray();
			var mailItemsColumns = new[] { "MailId", "ItemCode", "ItemCount" };

			var count = await _queryFactory.Query("mail_item").InsertAsync(mailItemsColumns, mailItemsData);

			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateInAppMailFailInsertItem, GameUserId = gameUserId, MailId = mailId },
					"CreateInAppMailFailInsertItem");

				return ErrorCode.CreateInAppMailFailInsertItem;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.CreateInAppMailFailException, GameUserId = gameUserId, MailId = mailId },
				"CreateInAppMailFailException");

			return ErrorCode.CreateInAppMailFailException;
		}
		
	}

	private ErrorCode VerifyUserAndCheckIsReceived(Int32 gameUserId, Int32 mailOwnerId, Int64 mailId, Boolean isReceived)
	{
		if (mailOwnerId != gameUserId)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateMailStatusToReceivedWrongGameUserId, GameUserId = gameUserId, MailId = mailId, },
				"UpdateMailStatusToReceivedWrongGameUserId");

			return ErrorCode.UpdateMailStatusToReceivedWrongGameUserId;
		}

		if (isReceived == true)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.UpdateMailStatusToReceivedFailAlreadyReceived, GameUserId = gameUserId, MailId = mailId }, 
				"UpdateMailStatusToReceivedFailAlreadyReceived");

			return ErrorCode.UpdateMailStatusToReceivedFailAlreadyReceived;
		}

		return ErrorCode.None;
	}


	private async Task RollbackCreateMailAsync(long mailId)
	{
		if (mailId == 0)
		{
			return;
		}

		try
		{
			var count = await _queryFactory.Query("mail").Where("MailId", "=", mailId).DeleteAsync();

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackCreateMailFailDelete, MailId = mailId }, "RollbackCreateMailFailDelete");
			}
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackCreateMailFailException, MailId = mailId }, "RollbackCreateMailFailException");
		}
	}

}