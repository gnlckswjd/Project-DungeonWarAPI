using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IInAppPurchaseDataCRUD
{
	public Task<(ErrorCode, Int32)> InsertReceiptAsync(Int32 gameUserId, String purchaseId, Int32 packageId);

	public Task<ErrorCode> RollbackStoreReceiptAsync(Int32 receiptId);
}