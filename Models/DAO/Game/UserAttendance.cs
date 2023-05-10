namespace DungeonWarAPI.Models.DAO.Game;

public class UserAttendance
{
	public Int32 GameUserId { get; set; }
	public Int16 AttendanceCount { get; set; }
	public DateTime LastLoginDate { get; set; }
}