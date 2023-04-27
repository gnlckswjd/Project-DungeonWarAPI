using DungeonWarAPI;

namespace DungeonWarAPI.Services;

public interface IMemoryDatabase
{
	Task<ErrorCode> RegisterUserAsync(string id, string authToken, Int64 accountID);
	Task<Tuple<ErrorCode, List<string>>> GetNoticeAsync();
}