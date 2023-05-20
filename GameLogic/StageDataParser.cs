using DungeonWarAPI.Enum;

namespace DungeonWarAPI.GameLogic;

public static class StageDataParser
{
	public static ( List<(Int32, Int32)> itemList, List<(Int32, Int32)> npcList, Int32 stageLevel) ParseStageData(
		Dictionary<String, Int32> data)
	{
		var stageItemKeyPrefix = MemoryDatabaseKeyGenerator.GetStageItemKeyPrefix();
		var stageNpcKeyPrefix = MemoryDatabaseKeyGenerator.GetStageNpcKeyPrefix();
		var stageLevelKeyPrefix = MemoryDatabaseKeyGenerator.GetStageLevelKeyPrefix();


		List<(Int32, Int32)> itemList = new List<(Int32, Int32)>();
		List<(Int32, Int32)> npcList = new List<(Int32, Int32)>();
		Int32 stageLevel = 0;

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

		return (itemList, npcList, stageLevel);
	}


	private static void ParseAndAddItem(String key, Int32 itemCount, List<(Int32, Int32)> itemList, String stageItemKeyPrefix)
	{
		if (itemCount == 0)
		{
			return;
		}

		Int32 itemCode = Int32.Parse(key.Substring(stageItemKeyPrefix.Length));

		switch (itemCode)
		{
			case (Int32)ItemCode.Gold:
			case (Int32)ItemCode.Potion:
			{
				itemList.Add((itemCode, itemCount));
				break;
			}

			default:
			{
				for (Int32 i = 0; i < itemCount; i++)
				{
					itemList.Add((itemCode, 1));
				}

				break;
			}
		}
	}
}