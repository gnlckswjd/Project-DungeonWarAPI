using DungeonWarAPI.Enum;

namespace DungeonWarAPI.GameLogic;

public static class ItemEnhancer
{
    public static ErrorCode CheckEnhancementPossibility(Int32 maxEnhancementCount, Int32 enhancementCount, Int32 attributeCode)
    {
        if (maxEnhancementCount == -1)
        {
            return ErrorCode.WrongItemCode;
        }

        if (maxEnhancementCount == 0)
        {
            return ErrorCode.CanNotEnhancement;
        }

        if (maxEnhancementCount <= enhancementCount)
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
        Double randomNumber = random.NextDouble();
        return randomNumber < 0.3;
    }

    public static Int32 GetAttackPower(Int32 baseAttack)
    {
        Double interestRate = 1.1;
        return (int)Math.Round(baseAttack * interestRate);
    }
    public static Int32 GetDefensePower(Int32 baseDefense)
    {
        Double interestRate = 1.1;
        return (int)Math.Round(baseDefense * interestRate);
    }
}