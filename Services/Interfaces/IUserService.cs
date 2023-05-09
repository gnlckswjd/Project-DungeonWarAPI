using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IUserService
{
    public Task<(ErrorCode, Int32)> CreateUserAsync(Int32 accountId);
    public Task<ErrorCode> CreateUserAttendanceAsync(Int32 gameUserId);
    public Task<ErrorCode> RollbackCreateUserAttendanceAsync(Int32 gameUserId);
	public Task<ErrorCode> CreateUserItemAsync(Int32 gameId);
    public Task<ErrorCode> RollbackCreateUserAsync(Int32 gameId);
    public Task<(ErrorCode, UserData)> LoadUserDataAsync(Int32 playerId);
    public Task<(ErrorCode, List<OwnedItem>)> LoadUserItemsAsync(Int32 gameUserId);
}