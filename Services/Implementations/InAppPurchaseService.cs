using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class InAppPurchaseService : IInAppPurchaseService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<InAppPurchaseService> _logger;
	private readonly MasterDataManager _masterData;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public InAppPurchaseService(ILogger<InAppPurchaseService> logger,
		IOptions<DatabaseConfiguration> configurationOptions,
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

	public async Task<(ErrorCode, Int32)> StoreReceiptAsync(Int32 gameUserId, String receiptSerialCode, Int32 packageId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ReceiptSerialCode = receiptSerialCode },
			"StoreReceipt Start");

		try
		{
			var existingReceipt = await _queryFactory.Query("receipt")
				.Where("ReceiptSerialCode", receiptSerialCode)
				.FirstOrDefaultAsync();

			if (existingReceipt != null)
			{
				_logger.ZLogErrorWithPayload(new
					{
						ErrorCode = ErrorCode.StoreReceiptFailDuplicatedReceipt,
						GameUserId = gameUserId,
						ReceiptSerialCode = receiptSerialCode
					}
					, "StoreReceiptFailDuplicatedReceipt");
				return (ErrorCode.StoreReceiptFailDuplicatedReceipt, 0);
			}

			var receiptId = await _queryFactory.Query("receipt")
				.InsertGetIdAsync<int>(new
				{
					GameUserId = gameUserId,
					ReceiptSerialCode = receiptSerialCode,
					PurchaseDate = DateTime.UtcNow,
					PackageId = packageId
				});

			if (receiptId < 1)
			{
				_logger.ZLogErrorWithPayload(new
					{
						ErrorCode = ErrorCode.StoreReceiptFailInsert,
						GameUserId = gameUserId,
						ReceiptSerialCode = receiptSerialCode
					}
					, "StoreReceiptFailInsert");
				return (ErrorCode.StoreReceiptFailInsert, 0);
			}

			return (ErrorCode.None, receiptId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new
				{
					ErrorCode = ErrorCode.StoreReceiptFailException,
					GameUserId = gameUserId,
					ReceiptSerialCode = receiptSerialCode
				}
				, "StoreReceiptFailException");
			return (ErrorCode.StoreReceiptFailException, 0);
		}
	}

	public async Task<ErrorCode> CreateInAppMailAsync(Int32 gameUserId, List<PackageItem> packageItems)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "CreateInAppMail Start");
		long mailId = 0;
		try
		{
			mailId = await _queryFactory.Query("mail").InsertGetIdAsync<Int64>(
				new
				{
					GameUserId = gameUserId,
					// PackageId는 모두 동일하기 때문에 어느 인덱스에 접근하든 무방
					Title = packageItems[0].PackageId + "구매 지급",
					Contents = packageItems[0].PackageId + "구매 지급 안내",
					isRead = false,
					isReceived = false,
					isInApp = true,
					isRemoved = false,
				});
			if (mailId < 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.CreateInAppMailFailInsertMail,
						GameUserId = gameUserId,
						MailId = mailId
					},
					"CreateInAppMailFailInsertMail");
				return ErrorCode.CreateInAppMailFailInsertMail;
			}

			var mailItemsData = packageItems.Select(item => new object[] { mailId, item.ItemCode, item.ItemCount })
				.ToArray();
			var mailItemsColumns = new[] { "MailId", "ItemCode", "ItemCount" };

			var count = await _queryFactory.Query("mail_item").InsertAsync(mailItemsColumns, mailItemsData);


			if (count < 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.CreateInAppMailFailInsertItem,
						GameUserId = gameUserId,
						MailId = mailId
					},
					"CreateInAppMailFailInsertItem");

				await RollbackCreateMailAsync(mailId);

				return ErrorCode.CreateInAppMailFailInsertItem;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new
				{
					ErrorCode = ErrorCode.CreateInAppMailFailException,
					GameUserId = gameUserId,
					MailId = mailId
				},
				"CreateInAppMailFailException");
			await RollbackCreateMailAsync(mailId);
			return ErrorCode.CreateInAppMailFailException;
		}
	}

	public async Task<ErrorCode> RollbackStoreReceiptAsync(Int32 receiptId)
	{
		if (receiptId == 0)
		{
			return ErrorCode.RollbackStoreReceiptFailWrongId;
		}

		try
		{
			var count = await _queryFactory.Query("receipt").Where("ReceiptId", "=", receiptId).DeleteAsync();

			if (count != 1)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.RollbackStoreReceiptFailDelete, Receipt = receiptId },
					"RollbackStoreReceiptFailDelete");
				return ErrorCode.RollbackStoreReceiptFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.RollbackStoreReceiptFailException, Receipt = receiptId },
				"RollbackStoreReceiptFailException");
			return ErrorCode.RollbackStoreReceiptFailException;
		}
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
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.RollbackCreateMailFailDelete, MailId = mailId },
					"RollbackCreateMailFailDelete");
			}
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.RollbackCreateMailFailException, MailId = mailId },
				"RollbackCreateMailFailException");
		}
	}
}