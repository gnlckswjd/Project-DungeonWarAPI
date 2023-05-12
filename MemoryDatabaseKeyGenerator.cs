namespace DungeonWarAPI
{
    public class MemoryDatabaseKeyGenerator
    {
        private const string loginUID = "UID_";
        private const string lockKey = "ULock_";
        private const string mailPageKey = "UMailPage_";
        private const string stageKey = "UStage_";
        private const string stageLevelKey = "UStageLevel";
        private const string stageItemKey = "UStageItem_";
        private const string stageNpcKey = "UStageNpc_";


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
        public static string MakeStageKey(string id)
        {
            return stageKey + id;
        }

        public static string MakeStageLevelKey()
        {
            return stageLevelKey;
        }
        public static string MakeStageItemKey(int id)
        {
            return stageItemKey + id;
        }

        public static string MakeStageNpcKey(int id)
        {
            return stageNpcKey + id;
        }
    }
}
