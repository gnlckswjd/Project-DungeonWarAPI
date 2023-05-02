namespace DungeonWarAPI.Models.Database.Game;

public class MailItem
{
    //public Int64 MailItemId { get; set; }
    public long MailId { get; set; }
    public int ItemCode { get; set; }
    public int ItemCount { get; set; }
    public int EnhancedCount { get; set; }
}