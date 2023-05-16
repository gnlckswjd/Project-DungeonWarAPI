using DungeonWarAPI.Enum;

namespace DungeonWarAPI.GameLogic;

public static class StageDataParser
{
	public static (Int32 stageLevel, List<(Int32, Int32)> itemList, List<(Int32, Int32)> npcList) ParseStageData(
		Dictionary<String, Int32> data)
	{
		Int32 stageLevel = 0;
		List<(Int32, Int32)> itemList = new List<(Int32, Int32)>();
		List<(Int32, Int32)> npcList = new List<(Int32, Int32)>();

		var stageItemKeyPrefix = MemoryDatabaseKeyGenerator.GetStageItemKeyPrefix();
		var stageNpcKeyPrefix = MemoryDatabaseKeyGenerator.GetStageNpcKeyPrefix();
		var stageLevelKeyPrefix = MemoryDatabaseKeyGenerator.GetStageLevelKeyPrefix();

		foreach (var pair in data)
		{
			String key = pair.Key;
			Int32 value = pair.Value;

			if (key.StartsWith(stageItemKeyPrefix))
			{
				ParseAndAddItem(key, value, itemList, stageItemKeyPrefix);
			}
			else if (key.StartsWith(stageNpcKeyPrefix))
			{
				Int32 code = int.Parse(key.Substring(stageNpcKeyPrefix.Length));
				npcList.Add((code, value));
			}
			else if (key.StartsWith(stageLevelKeyPrefix))
			{
				stageLevel = value;
			}
		}

		return (stageLevel, itemList, npcList);
	}


	private static void ParseAndAddItem(String key, Int32 value, List<(Int32, Int32)> itemList, String stageItemKeyPrefix)
	{
		Int32 code = Int32.Parse(key.Substring(stageItemKeyPrefix.Length));

		switch (code)
		{
			case (Int32)ItemCode.Gold:
			case (Int32)ItemCode.Potion:
			{
				itemList.Add((code, value));
				break;
			}

			default:
			{
				for (Int32 i = 0; i < value; i++)
				{
					itemList.Add((code, 1));
				}

				break;
			}
		}
	}
}