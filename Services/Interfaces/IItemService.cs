namespace DungeonWarAPI.Services.Interfaces;

public interface IItemService
{
    public Task<(ErrorCode, Int32 itemCode, Int32 enhancementCount)> LoadItemAsync(Int32 gameUserId, Int64 itemId);

    public Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold);

    public Task<ErrorCode> UpdateEnhancementCountAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount);

    public Task<ErrorCode> DestroyItemAsync(Int32 gameUserId, Int64 itemId);

    public Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Boolean isSuccess);

    public Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold);

    public Task<ErrorCode> RollbackUpdateEnhancementCountAsync(Int64 itemId);

    public Task<ErrorCode> RollbackDestroyItemAsync(Int64 itemId);
}