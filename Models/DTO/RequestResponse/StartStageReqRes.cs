using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO.RequestResponse;


public class StartStageRequest
{
	public String Email { get; set; }
	public Int32 SelectedStageLevel { get; set; }
}
public class StartStageResponse
{
	public ErrorCode Error { get; set; }	
	public List<StageItem> ItemList {get; set; }
	public List<StageNpc> NpcList { get; set; }
}
