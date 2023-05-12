using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class StageEndRequest
{
	public String Email { get; set; }
}

public class StageEndResponse
{
	public Boolean IsCleared { get; set; }
	public ErrorCode Error { get; set; }
}