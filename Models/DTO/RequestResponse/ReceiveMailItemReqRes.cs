using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class ReceiveMailItemRequest
{
    public long MailId { get; set; }
}
public class ReceiveMailItemResponse
{
    public ErrorCode Error { get; set; }
}