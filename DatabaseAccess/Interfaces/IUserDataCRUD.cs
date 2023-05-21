using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IUserDataCRUD
{
    public Task<(ErrorCode, Int32)> CreateUserAsync(Int32 accountId);
    public Task<ErrorCode> CreateUserAttendanceAsync(Int32 gameUserId);
    public Task<ErrorCode> CreateUserStageAsync(Int32 gameUserId);

    public Task<(ErrorCode, UserData)> LoadUserDataAsync(Int32 playerId);
    public Task<(ErrorCode, List<OwnedItem>)> LoadUserItemsAsync(Int32 gameUserId);
    public Task<(ErrorCode, Int64 gold)> LoadGoldAsync(Int32 gameUserId);

    public Task<ErrorCode> UpdateGoldAsync(Int32 gameUserId, Int32 gold);
	public Task<(ErrorCode, Int32 existingLevel, Int32 existingExp)> UpdateExpAsync(Int32 gameUserId, Int32 exp);

	public Task<ErrorCode> RollbackCreateUserAttendanceAsync(Int32 gameUserId);
	public Task<ErrorCode> RollbackCreateUserStageAsync(Int32 gameUserId);
    public Task<ErrorCode> RollbackCreateUserAsync(Int32 gameId);
    public Task<ErrorCode> RollbackUpdateMoneyAsync(Int32 gameUserId, Int32 gold);

}