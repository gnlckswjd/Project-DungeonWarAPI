namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class DeleteMailRequest
{
    public int MailId { get; set; }
}


public class DeleteMailResponse
{
    public ErrorCode Error { get; set; }
}