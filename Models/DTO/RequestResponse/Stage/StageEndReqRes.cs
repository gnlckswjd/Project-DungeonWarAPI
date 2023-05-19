using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Stage;

public class StageEndRequest
{
    public string Email { get; set; }
}

public class StageEndResponse
{
    public bool IsCleared { get; set; }
    public ErrorCode Error { get; set; }
}