namespace DungeonWarAPI.Services;

public interface IAccountDatabase : IDisposable
{
	public Task<ErrorCode> CreateAccountAsync(String id, String password, Byte[] guid);
	public Task<ErrorCode> RollbackAccountAsync(Byte[] guid);
	public Task<Tuple<ErrorCode, Byte[]>> VerifyAccount(String id, String password);
}