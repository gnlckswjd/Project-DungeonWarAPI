using DungeonWarAPI.Models.DAO.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface IAttendanceRewardService
{
	public Task<(ErrorCode, int)> LoadAttendanceCountAsync(int gameUserId);
    public Task<(ErrorCode, DateTime lastLoginDate, Int16 attendanceCount)> UpdateLoginDateAsync(Int32 gameUserId);
    public Task<ErrorCode> CreateAttendanceRewardMailAsync(Int32 gameUserId, AttendanceReward reward);
    public Task<ErrorCode> RollbackLoginDateAsync(Int32 gameUserId, DateTime lastLoginDate, Int16 attendanceCount);
}