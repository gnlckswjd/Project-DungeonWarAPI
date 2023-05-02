namespace DungeonWarAPI.Models.Database.Game;

public class Mail
{
    public const int MailCountInPage = 20;
    public long MailId { get; set; }
    //public Int32 GameUserId { get; set; }
    public string Title { get; set; }
    public string Contents { get; set; }
    public bool IsRead { get; set; }
    public bool IsReceived { get; set; }
    public DateTime ExpirationDate { get; set; }

}