namespace DungeonWarAPI.Models.Database.Game;

public class Mail
{
    public const Int32 MailCountInPage = 20;
    public Int64 MailId { get; set; }
    public Int32 GameUserId { get; set; }
    public String Title { get; set; }
    public String Contents { get; set; }
    public Boolean IsRead { get; set; }
    public Boolean IsReceived { get; set; }
    public Boolean IsInApp { get; set; }
    public Boolean IsRemoved { get; set; }
    public DateTime ExpirationDate { get; set; }

}