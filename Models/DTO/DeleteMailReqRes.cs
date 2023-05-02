namespace DungeonWarAPI.Models.DTO;

public class DeleteMailRequest
{
    public Int32 MailId { get; set; }
}


public class DeleteMailResponse
{
    public ErrorCode Result { get; set; }
}