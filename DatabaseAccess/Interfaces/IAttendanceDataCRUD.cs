using DungeonWarAPI.Enum;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IAttendanceDataCRUD
{
	public Task<(ErrorCode, Int32)> LoadAttendanceCountAsync(Int32 gameUserId);

    public Task<(ErrorCode, DateTime lastLoginDate, Int16 attendanceCount)> UpdateLoginDateAsync(Int32 gameUserId);

    public Task<ErrorCode> RollbackLoginDateAsync(Int32 gameUserId, DateTime lastLoginDate, Int16 attendanceCount);
}