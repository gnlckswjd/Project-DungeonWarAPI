using DungeonWarAPI.Game;

namespace DungeonWarAPI.ModelPacket;

public class MailReadRequest
{
	public Int32 MailId { get; set; }

}

public class MailReadResponse
{
	public ErrorCode Result { get; set; }

}