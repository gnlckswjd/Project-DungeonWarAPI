namespace DungeonWarAPI.Services;

public interface IGameDatabase : IDisposable
{
	public Task<ErrorCode> CreateUserAsync(byte[] guid);
}