using DungeonWarAPI.Enum;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IStageDataCRUD
{
	public Task<(ErrorCode, Int32)> LoadStageListAsync(Int32 gameUserId);

	public Task<(ErrorCode, Boolean isIncrement)> IncreaseMaxClearedStageAsync(Int32 gameUserId, Int32 clearLevel);


	public Task<ErrorCode> RollbackUpdateExpAsync(Int32 gameUserId, Int32 level, Int32 exp);
	public Task<ErrorCode> RollbackIncreaseMaxClearedStageAsync(Int32 gameUserId, Boolean isIncrement);
}