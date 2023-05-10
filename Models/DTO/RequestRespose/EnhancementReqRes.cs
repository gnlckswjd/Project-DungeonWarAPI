using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class EnhancementRequest
{
    public long ItemId { get; set; }

}

public class EnhancementResponse
{
    public ErrorCode Error { get; set; }

    public bool EnhancementResult { get; set; }
}