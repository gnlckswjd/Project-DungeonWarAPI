using DungeonWarAPI;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services;

public interface IMemoryDatabase
{
	Task<ErrorCode> RegisterUserAsync(string id, string authToken, UserData userData);
	Task<(ErrorCode, List<string>)> LoadNotificationsAsync();

	Task<(ErrorCode, AuthUserData )> LoadAuthUserDataAsync(String email);

	Task<ErrorCode> LockUserRequestAsync(String key, String authToken);
	Task<ErrorCode> UnLockUserRequestAsync(String key);

	Task<ErrorCode> StoreUserMailPageAsync(AuthUserData authUserData, Int32 pageNumber);
}