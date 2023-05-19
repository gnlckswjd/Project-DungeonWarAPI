using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Stage;


public class StageStartRequest
{
    public string Email { get; set; }
    public int SelectedStageLevel { get; set; }
}
public class StageStartResponse
{
    public ErrorCode Error { get; set; }
    public List<StageItem> ItemList { get; set; }
    public List<StageNpc> NpcList { get; set; }
}
