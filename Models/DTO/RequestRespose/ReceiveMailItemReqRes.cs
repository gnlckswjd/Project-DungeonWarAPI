using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class ReceiveMailItemRequest
{
    public long MailId { get; set; }
}
public class ReceiveMailItemResponse
{
    public ErrorCode Error { get; set; }
}