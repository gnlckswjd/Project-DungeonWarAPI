namespace DungeonWarAPI.Services.Interfaces;

public interface IItemService
{
    public Task<(ErrorCode, int itemCode, int enhancementCount)> LoadItemAsync(int gameUserId, long itemId);

    public Task<ErrorCode> UpdateGoldAsync(int gameUserId, int gold);

    public Task<ErrorCode> UpdateEnhancementCountAsync(int gameUserId, long itemId, int enhancementCount);

    public Task<ErrorCode> DestroyItemAsync(int gameUserId, long itemId);

    public Task<ErrorCode> InsertEnhancementHistoryAsync(int gameUserId, long itemId, int enhancementCount, bool isSuccess);

    public Task<ErrorCode> RollbackUpdateMoneyAsync(int gameUserId, int gold);

    public Task<ErrorCode> RollbackUpdateEnhancementCountAsync(long itemId);

    public Task<ErrorCode> RollbackDestroyItemAsync(long itemId);
}