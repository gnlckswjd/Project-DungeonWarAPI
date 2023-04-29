using DungeonWarAPI.ModelDatabase;

namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<Tuple<ErrorCode, Int32>> CreateUserAsync(Int32 accountId);
	public Task<ErrorCode> CreateUserItemAsync(Int32 gameId);
	public Task<ErrorCode> RollbackUserAsync(Int32 gameId);
}