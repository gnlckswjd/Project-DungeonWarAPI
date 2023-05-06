using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO;

public class MailListRequest
{
    public int PageNumber { get; set; }
}

public class MailListResponse
{
    public ErrorCode Error { get; set; }
    public List<MailWithItems> MailsWithItems { get; set; }

}