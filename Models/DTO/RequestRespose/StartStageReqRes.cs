using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Models.DTO.RequestRespose;


public class StartStageRequest
{
	public Int32 SelectedStageLevel { get; set; }
}
public class StartStageResponse
{
	public ErrorCode Error { get; set; }	
	public List<StageItem> ItemList {get; set; }
	public List<StageNpc> NpcList { get; set; }
}
