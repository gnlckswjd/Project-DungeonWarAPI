using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class DeleteMailRequest
{
    public int MailId { get; set; }
}


public class DeleteMailResponse
{
    public ErrorCode Error { get; set; }
}