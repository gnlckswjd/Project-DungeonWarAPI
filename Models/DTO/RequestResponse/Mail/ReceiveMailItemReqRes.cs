using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Mail;

public class ReceiveMailItemRequest
{
    public Int64 MailId { get; set; }
}
public class ReceiveMailItemResponse
{
    public ErrorCode Error { get; set; }
}