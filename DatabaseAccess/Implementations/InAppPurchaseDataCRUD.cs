using System.Data;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.Database.Game;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class InAppPurchaseDataCRUD : DatabaseAccessBase, IInAppPurchaseDataCRUD
{
	public InAppPurchaseDataCRUD(ILogger<InAppPurchaseDataCRUD> logger, QueryFactory queryFactory)
		:base(logger,queryFactory)
	{
		
	}


	public async Task<(ErrorCode, Int32)> InsertReceiptAsync(Int32 gameUserId, String receiptSerialCode, Int32 packageId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, ReceiptSerialCode = receiptSerialCode }, "InsertReceipt Start");

		try
		{
			var existingReceipt = await _queryFactory.Query("receipt")
				.Where("ReceiptSerialCode", receiptSerialCode)
				.FirstOrDefaultAsync();

			if (existingReceipt != null)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.InsertReceiptFailDuplicatedReceipt, GameUserId = gameUserId, ReceiptSerialCode = receiptSerialCode },
					"InsertReceiptFailDuplicatedReceipt");

				return (ErrorCode.InsertReceiptFailDuplicatedReceipt, 0);
			}

			var receiptId = await _queryFactory.Query("receipt")
				.InsertGetIdAsync<Int32>(new { GameUserId = gameUserId, ReceiptSerialCode = receiptSerialCode, PurchaseDate = DateTime.UtcNow, PackageId = packageId });

			if (receiptId < 1)
			{
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.InsertReceiptFailInsert, GameUserId = gameUserId, ReceiptSerialCode = receiptSerialCode },
					"InsertReceiptFailInsert");

				return (ErrorCode.InsertReceiptFailInsert, 0);
			}

			return (ErrorCode.None, receiptId);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.InsertReceiptFailException, GameUserId = gameUserId, ReceiptSerialCode = receiptSerialCode },
				"InsertReceiptFailException");

			return (ErrorCode.InsertReceiptFailException, 0);
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
				_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.RollbackStoreReceiptFailDelete, Receipt = receiptId }, "RollbackStoreReceiptFailDelete");

				return ErrorCode.RollbackStoreReceiptFailDelete;
			}

			return ErrorCode.None;
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(e, new { ErrorCode = ErrorCode.RollbackStoreReceiptFailException, Receipt = receiptId }, "RollbackStoreReceiptFailException");

			return ErrorCode.RollbackStoreReceiptFailException;
		}
	}

}