using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class AttendanceListRequest
{
}

public class AttendanceListResponse
{
    public ErrorCode Error { get; set; }
    public int AttendanceCount { get; set; }
}