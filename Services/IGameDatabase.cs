namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<ErrorCode> CreateUserAsync(Byte[] guid);
	public Task<ErrorCode> CreateUserItemAsync(Byte[] guid);
	public Task<ErrorCode> RollbackUserAsync(Byte[] guid);
}