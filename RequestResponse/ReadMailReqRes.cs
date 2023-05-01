using DungeonWarAPI.Game;

namespace DungeonWarAPI.ModelPacket;

public class ReadMailRequest
{
	public Int32 MailId { get; set; }

}

public class ReadMailResponse
{
	public ErrorCode Result { get; set; }

}