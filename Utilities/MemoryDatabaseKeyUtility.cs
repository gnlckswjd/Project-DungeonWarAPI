namespace DungeonWarAPI.Utilities
{
    public class MemoryDatabaseKeyUtility
    {
        const string loginUID = "UID_";
        const string lockKey = "ULock_";
        private const string mailPageKey = "UMailPage_";

        public static string MakeUIDKey(string id)
        {
            return loginUID + id;
        }

        public static string MakeUserLockKey(string id)
        {
            return lockKey + id;
        }

        public static string MakeMailPageKey(string id)
        {
            return mailPageKey + id;
        }
    }
}
