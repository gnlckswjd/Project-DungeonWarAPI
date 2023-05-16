using DungeonWarAPI.Enum;

namespace DungeonWarAPI;

public class MemoryDatabaseKeyGenerator
{
	private const String loginKeyPrefix = "UID_";
	private const String lockKeyPrefix = "ULock_";
				  
	private const String stageKeyPrefix = "UStage_";
	private const String stageLevelKeyPrefix = "UStageLevel";
	private const String stageItemKeyPrefix = "UStageItem_";
	private const String stageNpcKeyPrefix = "UStageNpc_";


	public static String MakeUIDKey(String id)
	{
		return loginKeyPrefix + id;
	}

	public static String MakeUserLockKey(String id)
	{
		return lockKeyPrefix + id;
	}

	public static String MakeStageKey(String id)
	{
		return stageKeyPrefix + id;
	}

	public static String MakeStageLevelKey()
	{
		return stageLevelKeyPrefix;
	}

	public static String MakeStageItemKey(Int32 id)
	{
		return stageItemKeyPrefix + id;
	}

	public static String MakeStageNpcKey(Int32 id)
	{
		return stageNpcKeyPrefix + id;
	}

	public static String GetStageItemKeyPrefix()
	{
		return stageItemKeyPrefix;
	}

	public static String GetStageNpcKeyPrefix()
	{
		return stageNpcKeyPrefix;
	}

	public static String GetStageLevelKeyPrefix()
	{
		return stageNpcKeyPrefix;
	}

}