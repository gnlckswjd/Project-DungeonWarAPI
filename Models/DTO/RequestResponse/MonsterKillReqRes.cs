using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class MonsterKillRequest
{
	public Int32 NpcCode { get; set; }

	public String Email { get; set; }
}
public class MonsterKillResponse
{
	public ErrorCode Error { get; set; }
}