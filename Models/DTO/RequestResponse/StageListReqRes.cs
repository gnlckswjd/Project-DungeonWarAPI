using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class StageListRequest
{
	
}
public class StageListResponse
{
	public ErrorCode Error { get; set; }
	public Int32 MaxClearedStage { get; set; }
}