using DungeonWarAPI.Enum;

namespace DungeonWarAPI.GameLogic;

public static class ItemEnhancer
{
	private static Int32 cost = 500;

	private static Double interestRate = 1.1;
	public static (ErrorCode, Int32 cost)VerifyEnhancementPossibilityAndGetCost(Int32 maxEnhancementCount, Int32 enhancementCount, Int32 attributeCode, Int64 gold)
    {
        if (maxEnhancementCount == -1)
        {
            return (ErrorCode.WrongItemCode, 0);
        }

        if (maxEnhancementCount == 0)
        {
            return (ErrorCode.CanNotEnhancement, 0);
        }

        if (maxEnhancementCount <= enhancementCount)
        {
            return (ErrorCode.CanNotEnhancement, 0);
        }

        if (attributeCode != 1 && attributeCode != 2)
        {
            return (ErrorCode.CanNotEnhancement, 0);

        }

        if (gold < cost)
        {
	        return (ErrorCode.NotEnoughGold, 0);
        }

        return (ErrorCode.None, cost);
    }

    public static bool TryEnhancement()
    {
        Random random = new Random();
        Double randomNumber = random.NextDouble();
        return randomNumber < 0.3;
    }

    public static Int32 GetEnhancedAttackPower(Int32 baseAttack)
    {
	    return (Int32)Math.Round(baseAttack * interestRate);
    }
    public static Int32 GetEnhancedDefensePower(Int32 baseDefense)
    {
	    return (Int32)Math.Round(baseDefense * interestRate);
    }
}