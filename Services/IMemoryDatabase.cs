using DungeonWarAPI;
using DungeonWarAPI.Game;

namespace DungeonWarAPI.Services;

public interface IMemoryDatabase
{
	Task<ErrorCode> RegisterUserAsync(string id, string authToken, UserData userData);
	Task<(ErrorCode errorCode, List<string> notifications)> LoadNotificationsAsync();
}