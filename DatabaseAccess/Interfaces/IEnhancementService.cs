using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IEnhancementService
{
    public Task<ErrorCode> ValidateEnoughGoldAsync(Int32 gameUserId, Int64 cost);
    public Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold);
	public Task<ErrorCode> UpdateEnhancementResultAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Int32 attributeCode, Int32 attack, Int32 defense);
    public Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Boolean isSuccess);
   
    public Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold);
    public Task<ErrorCode> RollbackUpdateEnhancementCountAsync(Int64 itemId, Int32 attributeCode, Int32 attack, Int32 defense, Int32 enhancementCount);


}