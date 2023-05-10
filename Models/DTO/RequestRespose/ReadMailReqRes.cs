using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class ReadMailRequest
{
    public int MailId { get; set; }

}

public class ReadMailResponse
{
    public ErrorCode Error { get; set; }
    public String Content { get; set; }

}