using DungeonWarAPI.Models.DAO.Game;

namespace DungeonWarAPI.Services.Interfaces;

public interface ILoginRewardService
{
    public Task<(ErrorCode, DateTime lastLoginDate, short attendanceCount)> UpdateLoginDateAsync(int gameUserId);
    public Task<ErrorCode> CreateAttendanceRewardMailAsync(int gameUserId, AttendanceReward reward);
    public Task<ErrorCode> RollbackLoginDateAsync(int gameUserId, DateTime lastLoginDate, short attendanceCount);
}