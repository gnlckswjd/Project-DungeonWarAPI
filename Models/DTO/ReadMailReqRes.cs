namespace DungeonWarAPI.Models.DTO;

public class ReadMailRequest
{
    public int MailId { get; set; }

}

public class ReadMailResponse
{
    public ErrorCode Error { get; set; }

}