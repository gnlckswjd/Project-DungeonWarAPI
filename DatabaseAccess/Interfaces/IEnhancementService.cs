using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IEnhancementService
{
    public Task<(ErrorCode, OwnedItem)> LoadItemAsync(Int32 gameUserId, Int64 itemId);

    public Task<ErrorCode> ValidateEnoughGoldAsync(Int32 gameUserId, Int64 cost);

	public Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold);

    public Task<ErrorCode> UpdateEnhancementResultAsync(int gameUserId, long itemId, int enhancementCount,
	    int attributeCode, int attack, int defense);

    public Task<ErrorCode> DestroyItemAsync(Int32 gameUserId, Int64 itemId);

    public Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Boolean isSuccess);

    public Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold);

    public Task<ErrorCode> RollbackUpdateEnhancementCountAsync(long itemId, int attributeCode, int attack, int defense,
	    int enhancementCount);

    public Task<ErrorCode> RollbackDestroyItemAsync(Int64 itemId);
}