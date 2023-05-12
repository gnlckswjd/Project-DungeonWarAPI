using DungeonWarAPI.Enum;

namespace DungeonWarAPI;

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

	public static (Int32 stageLevel, List<(Int32, Int32)> itemList, List<(Int32, Int32)> npcList) ParseStageData(
		Dictionary<String, Int32> data)
	{
		Int32 stageLevel = 0;
		List<(Int32, Int32)> itemList = new List<(Int32, Int32)>();
		List<(Int32, Int32)> npcList = new List<(Int32, Int32)>();

		foreach (var pair in data)
		{
			String key = pair.Key;
			Int32 value = pair.Value;

			if (key.StartsWith(stageItemKey))
			{
				ParseAndAddItem(key,value,itemList);
			}
			else if (key.StartsWith(stageNpcKey))
			{
				Int32 code = int.Parse(key.Substring(stageNpcKey.Length));
				npcList.Add((code, value));
			}
			else if (key.StartsWith(stageLevelKey))
			{
				stageLevel = value;
			}
		}

		return (stageLevel, itemList, npcList);
	}


	private static void ParseAndAddItem(String key, Int32 value, List<(Int32, Int32)> itemList)
	{
		Int32 code = int.Parse(key.Substring(stageItemKey.Length));

		switch (code)
		{
			case (int)ItemCode.Gold:
			case (int)ItemCode.Potion:
			{
				itemList.Add((code, value));
				break;
			}

			default:
			{
				for (int i = 0; i < value; i++)
				{
					itemList.Add((code, 1));
				}

				break;
			}
		}
	}
}