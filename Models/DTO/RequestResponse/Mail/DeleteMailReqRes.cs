using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Mail;

public class DeleteMailRequest
{
    public Int32 MailId { get; set; }
}


public class DeleteMailResponse
{
    public ErrorCode Error { get; set; }
}