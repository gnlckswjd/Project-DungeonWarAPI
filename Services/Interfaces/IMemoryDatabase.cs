using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IMemoryDatabase
{
    Task<ErrorCode> RegisterUserAsync(string id, string authToken, UserData userData);
    Task<(ErrorCode, List<string>)> LoadNotificationsAsync();

    Task<(ErrorCode, AuthUserData)> LoadAuthUserDataAsync(string email);

    Task<ErrorCode> LockUserRequestAsync(string key, string authToken);
    Task<ErrorCode> UnLockUserRequestAsync(string key);

    Task<ErrorCode> StoreUserMailPageAsync(AuthUserData authUserData, int pageNumber);
}