using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IInAppPurchaseService
{
    public Task<(ErrorCode, Int32)> StoreReceiptAsync(Int32 gameUserId, String purchaseId, Int32 packageId);
    public Task<ErrorCode> CreateInAppMailAsync(Int32 gameUserId, List<PackageItem> packageItems);
    public Task<ErrorCode> RollbackStoreReceiptAsync(Int32 receiptId);
}