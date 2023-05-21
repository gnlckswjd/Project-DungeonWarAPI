using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.GameLogic;

public static class StageInitializer
{
	public static ErrorCode VerifyAccessibility(Int32 maxClearedStage, Int32 selectedStageLevel)
	{
		if (maxClearedStage + 1 < selectedStageLevel)
		{
			return ErrorCode.VerifyStageAccessibilityFailExceedStageLevel;
		}

		return ErrorCode.None;
	}

	public static List<KeyValuePair<String,Int32>> CreateInitialKeyValue(List<StageItem> items, List<StageNpc> npcs, Int32 stageLevel)
	{
		var itemKeys = items.Select(item => MemoryDatabaseKeyGenerator.MakeStageItemKey(item.ItemCode));

		var npcKeys = npcs.Select(npc => MemoryDatabaseKeyGenerator.MakeStageNpcKey(npc.NpcCode));

		var list = itemKeys.Concat(npcKeys)
			.Select(key => new KeyValuePair<String, Int32>(key, 0))
			.ToList();

		list.Add(new KeyValuePair<String, Int32>(MemoryDatabaseKeyGenerator.MakeStageLevelKey(), stageLevel));

		return list;
	}
}