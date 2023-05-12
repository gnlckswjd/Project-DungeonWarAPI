using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Services.Interfaces;

public interface IDungeonStageService
{

	public Task<(ErrorCode, Int32)> LoadStageListAsync(Int32 gameUserId);
	public Task<ErrorCode> CheckStageAccessibilityAsync(Int32 gameUserId, Int32 selectedStageLevel);

	public Task<ErrorCode> ReceiveRewardItemAsync(Int32 gameUserId, List<(Int32, Int32)> itemCodeList);

	public Task<(ErrorCode,Int32 existingLevel, Int32 existingExp)> UpdateExpAsync(Int32 gameUserId, Int32 exp);

	public Task<ErrorCode> RollbackUpdateExpAsync(Int32 gameUserId, Int32 level, Int32 exp);
}