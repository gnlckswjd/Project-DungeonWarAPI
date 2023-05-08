using DungeonWarAPI.Models.DAO.Account;
using Microsoft.AspNetCore.SignalR;

namespace DungeonWarAPI.Managers;

public static class ItemEnhancementManager
{
    public static ErrorCode CheckEnhancementPossibility(int maxCount, int enhancementCount)
    {
        if (maxCount == -1)
        {
            return ErrorCode.WrongItemCode;
        }

        if (maxCount == 0)
        {
            return ErrorCode.CanNotEnhancement;
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

    public static int GetAttackPower(int baseAttack, int enhancementCount)
    {
        double interestRate = 1.1;
        double finalAttackPower = baseAttack * Math.Pow(interestRate, enhancementCount);
        return (int)finalAttackPower;
    }
    public static int GetDefensePower(int baseDefense, int enhancementCount)
    {
        double interestRate = 1.1;
        double finalDefensePower = baseDefense * Math.Pow(interestRate, enhancementCount);
        return (int)finalDefensePower;
    }
}