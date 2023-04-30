namespace DungeonWarAPI.Utils
{
	public class MemoryDatabaseKeyGenerator
	{
		const string loginUID = "UID_";
		const string lockKey = "ULock_";

		public static string MakeUIDKey(string id)
		{
			return loginUID + id;
		}

		public static string MakeUserLockKey(string id)
		{
			return lockKey + id;
		}
	}
}
