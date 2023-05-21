using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IEnhancementDataCRUD
{
	public Task<ErrorCode> UpdateEnhancementResultAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Int32 attributeCode, Int32 attack, Int32 defense);
    public Task<ErrorCode> InsertEnhancementHistoryAsync(Int32 gameUserId, Int64 itemId, Int32 enhancementCount, Boolean isSuccess);

}