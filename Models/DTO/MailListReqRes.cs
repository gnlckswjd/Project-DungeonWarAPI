using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO;

public class MailListRequest
{
    public int PageNumber { get; set; }
}

public class MailListResponse
{
    public ErrorCode Result { get; set; }
    public List<MailWithItems> MailsWithItems { get; set; }

}