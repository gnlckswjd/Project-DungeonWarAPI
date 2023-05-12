namespace DungeonWarAPI.Models.Database.Game;

public class MailItem
{
    //public Int64 MailItemId { get; set; }
    public Int64 MailId { get; set; }
    public Int32 ItemCode { get; set; }
    public Int32 ItemCount { get; set; }
    public Int32 EnhancementCount { get; set; }
}