using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.Payloads;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Mail;

public class MailListRequest
{
    public Int32 PageNumber { get; set; }
}

public class MailListResponse
{
    public ErrorCode Error { get; set; }
    public List<MailWithItems> MailWithItemsList { get; set; }

}