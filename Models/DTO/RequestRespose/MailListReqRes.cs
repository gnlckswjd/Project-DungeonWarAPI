using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.Payloads;

namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class MailListRequest
{
    public int PageNumber { get; set; }
}

public class MailListResponse
{
    public ErrorCode Error { get; set; }
    public List<MailWithItems> MailsWithItems { get; set; }

}