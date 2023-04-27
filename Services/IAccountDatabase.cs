using DungeonWarAPI;

namespace DungeonWarAPI.Services
{
	public interface IAccountDatabase : IDisposable
	{
		public Task<ErrorCode> CreateAccountAsync(String id, String password);

		public Task<ErrorCode> CreateDefaultDataAsync(String id, String password);
		public Task<Tuple<ErrorCode, Int64>> VerifyAccount(String id, String password);
	}
}