using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IInAppPurchaseService
{
    public Task<(ErrorCode, int)> StoreReceiptAsync(int gameUserId, string purchaseId, int packageId);
    public Task<ErrorCode> CreateInAppMailAsync(int gameUserId, List<PackageItem> packageItems);
    public Task<ErrorCode> RollbackStoreReceiptAsync(int receiptId);
}