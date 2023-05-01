namespace DungeonWarAPI.Game;

public class Mail
{
	public const Int32 MailCountInPage = 20;
	public Int64 MailId { get; set; }
	//public Int32 GameUserId { get; set; }
	public String Title { get; set; }
	public String Contents { get; set; }

	public Int32 ItemCode { get; set; }
	public Int32 ItemCount { get; set; }
	public Int32 EnhancedItem { get; set; }
	public Boolean IsRead { get; set; }
	public Boolean IsReceived { get; set; }
	public Int32 RemainingDay { get; set; }

	public struct MailItem
	{
		
	}
}