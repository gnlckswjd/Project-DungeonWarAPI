namespace DungeonWarAPI.Utils
{
	public class MemoryDatabaseKeyGenerator
	{
		const String loginUID = "UID_";
		const String lockKey = "ULock_";
		private const String mailPageKey = "UMailPage_";

		public static String MakeUIDKey(String id)
		{
			return loginUID + id;
		}

		public static String MakeUserLockKey(String id)
		{
			return lockKey + id;
		}

		public static String MakeMailPageKey(String id)
		{
			return mailPageKey + id;
		}
	}
}
