using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Mail;

public class ReadMailRequest
{
    public int MailId { get; set; }

}

public class ReadMailResponse
{
    public ErrorCode Error { get; set; }
    public string Content { get; set; }

}