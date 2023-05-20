using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Stage;

public class NpcKillRequest
{
    public Int32 NpcCode { get; set; }

    public String Email { get; set; }
}
public class NpcKillResponse
{
    public ErrorCode Error { get; set; }
}