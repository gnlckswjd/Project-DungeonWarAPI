using DungeonWarAPI;

namespace DungeonWarAPI.Services;

public interface IMemoryDatabase
{
	Task<ErrorCode> RegisterUserAsync(string id, string authToken, byte[] accountID);
	Task<Tuple<ErrorCode, List<string>>> LoadNoticeAsync();
}