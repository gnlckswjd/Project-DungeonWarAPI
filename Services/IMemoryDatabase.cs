using DungeonWarAPI;
using DungeonWarAPI.Game;
using DungeonWarAPI.ModelDatabase;

namespace DungeonWarAPI.Services;

public interface IMemoryDatabase
{
	Task<ErrorCode> RegisterUserAsync(string id, string authToken, UserData userData);
	Task<(ErrorCode, List<string>)> LoadNotificationsAsync();

	Task<(ErrorCode, AuthUserData )> LoadAuthUserDataAsync(String email);

	Task<ErrorCode> LockUserRequestAsync(String key, String authToken);
	Task<ErrorCode> UnLockUserRequestAsync(String key);
}