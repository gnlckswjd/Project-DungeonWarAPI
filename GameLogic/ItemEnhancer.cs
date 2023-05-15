﻿using DungeonWarAPI.Enum;

namespace DungeonWarAPI.GameLogic;

public static class ItemEnhancer
{
    public static ErrorCode CheckEnhancementPossibility(int maxCount, int enhancementCount, int attributeCode)
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

        if (attributeCode != 1 && attributeCode != 2)
        {
            return ErrorCode.CanNotEnhancement;

        }

        return ErrorCode.None;
    }

    public static bool TryEnhancement()
    {
        Random random = new Random();
        double randomNumber = random.NextDouble();
        return randomNumber < 0.3;
    }

    public static int GetAttackPower(int baseAttack)
    {
        double interestRate = 1.1;
        return (int)Math.Round(baseAttack * interestRate);
    }
    public static int GetDefensePower(int baseDefense)
    {
        double interestRate = 1.1;
        return (int)Math.Round(baseDefense * interestRate);
    }
}