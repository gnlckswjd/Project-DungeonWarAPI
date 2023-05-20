using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.GameLogic;

public static class StageRequestVerifier
{

	public static (Boolean, Int32) VerifyClearAndCalcExp(List<StageNpc> stageNpcs, List<(Int32, Int32)> killedNpcCodeAndCount)
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

	public static (ErrorCode, Int32 count) VerifyItemCount(Int32 itemCode, Int32 currentItemCount,Int32 requestAcquisitionCount, Int32 maxItemCount)
	{
		if (itemCode != (Int32) ItemCode.Gold && itemCode != (Int32)ItemCode.Potion)
		{
			requestAcquisitionCount = 1;
		}

		Int32 itemCount = currentItemCount + requestAcquisitionCount;

		if (itemCount > maxItemCount)
		{
			return (ErrorCode.ExceedItemCount, itemCount); 
		}

		return (ErrorCode.None,itemCount);
	}

}