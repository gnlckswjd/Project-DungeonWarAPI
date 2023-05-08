using DungeonWarAPI.Models.DAO.Account;
using Microsoft.AspNetCore.SignalR;

namespace DungeonWarAPI.Services;

public static class ItemManager
{
	public static ErrorCode CheckEnhancementPossibility(Int32 maxCount, Int32 enhancementCount)
	{
		if (maxCount == -1)
		{
			return ErrorCode.WrongItemCode;
		}

		if (maxCount == 0)
		{
			return  ErrorCode.CanNotEnhancement;
		}

		if (maxCount <= enhancementCount)
		{
			return ErrorCode.CanNotEnhancement;
		}

		return ErrorCode.None;
	}

	public static bool EnhanceItem()
    {
        Random random = new Random();
        double randomNumber = random.NextDouble();
        return randomNumber < 0.3;
    }

	public static Int32 GetAttackPower(Int32 baseAttack, Int32 enhancementCount)
	{
		double interestRate = 1.1;
		double finalAttackPower = baseAttack * Math.Pow(interestRate, enhancementCount);
		return (int)finalAttackPower;
	}
	public static Int32 GetDefensePower(Int32 baseDefense, Int32 enhancementCount)
	{
		double interestRate = 1.1;
		double finalDefensePower = baseDefense * Math.Pow(interestRate, enhancementCount);
		return (int)finalDefensePower;
	}
}