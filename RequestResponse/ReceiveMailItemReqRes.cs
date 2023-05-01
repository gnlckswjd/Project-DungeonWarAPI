namespace DungeonWarAPI.ModelPacket;

public class ReceiveMailItemRequest
{
	public Int64 MailId { get; set; }
}
public class ReceiveMailItemResponse
{
	public ErrorCode Result { get; set; }
}