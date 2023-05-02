namespace DungeonWarAPI.Models.DTO;

public class ReceiveMailItemRequest
{
    public long MailId { get; set; }
}
public class ReceiveMailItemResponse
{
    public ErrorCode Result { get; set; }
}