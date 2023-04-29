namespace DungeonWarAPI.Services;

public interface IAccountDatabase : IDisposable
{
	public Task<(ErrorCode errorCode, Int32 playerId)> CreateAccountAsync(String email, String password);
	public Task<ErrorCode> RollbackAccountAsync(Int32 accountId);
	public Task<(ErrorCode errorCode, Int32 playerId)> VerifyAccount(String email, String password);
}