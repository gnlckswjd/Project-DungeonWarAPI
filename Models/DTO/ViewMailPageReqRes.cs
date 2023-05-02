using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO;

public class ViewMailPageRequest
{
    public int PageNumber { get; set; }
}

public class ViewMailPageResponse
{
    public ErrorCode Result { get; set; }
    public List<Mail> Mails { get; set; }
}