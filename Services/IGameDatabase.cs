using DungeonWarAPI.Game;
using DungeonWarAPI.ModelDatabase;

namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<(ErrorCode errorCode, Int32 gameUserId)> CreateUserAsync(Int32 accountId);
	public Task<ErrorCode> CreateUserItemAsync(Int32 gameId);
	public Task<ErrorCode> RollbackUserAsync(Int32 gameId);

	public Task<(ErrorCode errorCode, UserData userData)> LoadUserData(Int32 playerId);
	public Task<(ErrorCode errorCode, List<OwnedItem> items)> LoadUserItems(Int32 gameUserId);
}