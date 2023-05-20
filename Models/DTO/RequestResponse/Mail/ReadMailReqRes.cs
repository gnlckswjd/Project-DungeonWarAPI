using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Mail;

public class ReadMailRequest
{
    public Int64 MailId { get; set; }

}

public class ReadMailResponse
{
    public ErrorCode Error { get; set; }
    public String Content { get; set; }

}