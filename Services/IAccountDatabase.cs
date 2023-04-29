namespace DungeonWarAPI.Services;

public interface IAccountDatabase : IDisposable
{
	public Task<Tuple<ErrorCode,Int32>> CreateAccountAsync(String email, String password);
	public Task<ErrorCode> RollbackAccountAsync(Int32 accountId);
	public Task<Tuple<ErrorCode, Int32>> VerifyAccount(String email, String password);
}