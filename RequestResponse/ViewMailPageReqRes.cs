using DungeonWarAPI.Game;

namespace DungeonWarAPI.ModelPacket;

public class ViewMailPageRequest
{
	public Int32  PageNumber { get; set; }
}

public class ViewMailPageResponse
{
	public ErrorCode Result { get; set; }
	public List<Mail> Mails { get; set; }	
}