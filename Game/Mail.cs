namespace DungeonWarAPI.Game;

public class Mail
{
	public Int64 MailId { get; set; }
	public Int32 GameUserId { get; set; }
	public String Title { get; set; }
	public String Contents { get; set; }
	public Int32 ItmeCode { get; set; }
	public Boolean IsRead { get; set; }
	public Boolean IsReceived { get; set; }
	public Int32 RemainingDay { get; set; }
}