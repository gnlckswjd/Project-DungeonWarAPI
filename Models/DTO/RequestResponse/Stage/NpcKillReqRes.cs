using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Stage;

public class NpcKillRequest
{
    public int NpcCode { get; set; }

    public string Email { get; set; }
}
public class NpcKillResponse
{
    public ErrorCode Error { get; set; }
}