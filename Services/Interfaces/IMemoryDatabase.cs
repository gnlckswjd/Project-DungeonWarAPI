using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IMemoryDatabase
{
    Task<ErrorCode> RegisterUserAsync(String id, String authToken, UserData userData);
    Task<(ErrorCode, List<String>)> LoadNotificationsAsync();

    Task<(ErrorCode, AuthUserData)> LoadAuthUserDataAsync(String email);

    Task<ErrorCode> LockUserRequestAsync(String key, String authToken);
    Task<ErrorCode> UnLockUserRequestAsync(String key);

    Task<ErrorCode> StoreUserMailPageAsync(AuthUserData authUserData, Int32 pageNumber);
}