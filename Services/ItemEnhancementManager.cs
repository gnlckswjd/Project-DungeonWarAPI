namespace DungeonWarAPI.Services;

public static class ItemEnhancementManager
{
    public static bool EnhanceItem()
    {
        Random random = new Random();
        double randomNumber = random.NextDouble();
        return randomNumber < 0.3;
    }
}