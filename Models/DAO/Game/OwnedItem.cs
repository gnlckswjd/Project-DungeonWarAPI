namespace DungeonWarAPI.Models.Database.Game;

public class OwnedItem
{
    public int ItemId { get; set; }
    //public Int32 GameUserId { get; set; }
    public int ItemCode { get; set; }
    public int EnhancementCount { get; set; }
    public int ItemCount { get; set; }
}