using DungeonWarAPI.Enum;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IAccountService : IDisposable
{
    public Task<(ErrorCode errorCode, Int32 playerId)> CreateAccountAsync(String email, String password);
    public Task<(ErrorCode errorCode, Int32 playerId)> VerifyAccount(String email, String password);

    public Task<ErrorCode> RollbackAccountAsync(Int32 accountId);
}