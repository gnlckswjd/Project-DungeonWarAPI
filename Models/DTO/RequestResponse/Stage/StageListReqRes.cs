using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Stage;

public class StageListRequest
{

}
public class StageListResponse
{
    public ErrorCode Error { get; set; }
    public int MaxClearedStage { get; set; }
}