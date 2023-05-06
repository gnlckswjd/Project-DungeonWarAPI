namespace DungeonWarAPI.Models.DTO;

public class EnhancementRequest
{
	public Int64 ItemId { get; set; }	

}

public class EnhancementResponse
{
	public ErrorCode Error { get; set; }

	public Boolean EnhancementResult { get; set; }
}