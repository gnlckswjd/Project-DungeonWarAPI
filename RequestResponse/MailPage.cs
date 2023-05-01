using DungeonWarAPI.Game;

namespace DungeonWarAPI.ModelPacket;

public class MailPageRequest
{
	public Int32  PageNumber { get; set; }
	public String  Email { get; set; }
}

public class MailPageResponse
{
	public ErrorCode Result { get; set; }
	public List<Mail> Mails { get; set; }	
}