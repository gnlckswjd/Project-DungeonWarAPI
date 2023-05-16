using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.GameLogic;

public static class StageClearEvaluator
{

	public static (Boolean, Int32) CheckClearAndCalcExp(List<StageNpc> stageNpcs, List<(Int32, Int32)> killedNpcCodeAndCount)
	{
		if (!stageNpcs.Any())
		{
			return (false, 0);
		}
		Int32 totalEarnedExp = 0;

		foreach (var stageNpc in stageNpcs)
		{
			var killedNpc = killedNpcCodeAndCount.FirstOrDefault(npcCodeAndCount => npcCodeAndCount.Item1 == stageNpc.NpcCode);

			if (killedNpc.Item2 != stageNpc.NpcCount)
			{
				return (false, 0);
			}

			totalEarnedExp += stageNpc.Exp * stageNpc.NpcCount;
		}

		return (true, totalEarnedExp);
	}

}