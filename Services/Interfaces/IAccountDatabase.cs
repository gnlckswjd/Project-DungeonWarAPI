namespace DungeonWarAPI.Services.Interfaces;

public interface IAccountDatabase : IDisposable
{
    public Task<(ErrorCode errorCode, int playerId)> CreateAccountAsync(string email, string password);
    public Task<ErrorCode> RollbackAccountAsync(int accountId);
    public Task<(ErrorCode errorCode, int playerId)> VerifyAccount(string email, string password);
}