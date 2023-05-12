using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class ItemAcquisitionRequest
{
	public Int32 ItemCode { get; set; }

	public Int32 ItemCount { get; set; }

	public String Email { get; set; }
}

public class ItemAcquisitionResponse
{
	public ErrorCode Error { get; set; }
}