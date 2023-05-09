namespace DungeonWarAPI.Models.DTO;

public class AttendanceListRequest
{
}

public class AttendanceListResponse
{
	public ErrorCode Error { get; set; }
	public Int32 AttendanceCount { get; set; }
}