using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Services.Interfaces;

public interface IDungeonStageService
{

	public Task<(ErrorCode, Int32)> LoadStageList(Int32 gameUserId);
}