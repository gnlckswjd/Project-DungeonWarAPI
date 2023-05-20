using DungeonWarAPI.Enum;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IAccountService : IDisposable
{
    public Task<(ErrorCode errorCode, Int32 playerId)> CreateAccountAsync(String email, String password);
    public Task<(ErrorCode errorCode, Int32 playerId)> VerifyAccount(String email, String password);

    public Task<ErrorCode> RollbackCreateAccountAsync(Int32 accountId);
}