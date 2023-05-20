using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.GameLogic;

public static class MailGenerator
{
	private const String rewardMailTitle = "일 출석 보상";
	private const String rewardMailContents = "일 출석 보상 지급 안내";
	public static Mail CreateAttendanceRewardMail(Int32 gameUserId, AttendanceReward reward)
	{
		return new Mail
		{
			GameUserId = gameUserId,
			Title = reward.AttendanceCount.ToString() + rewardMailTitle,
			Contents = reward.AttendanceCount.ToString() + rewardMailContents,
			IsRead = false,
			IsReceived = false,
			IsInApp = false,
			IsRemoved = false,
			ExpirationDate = DateTime.Today.AddDays(7).Date
		};

	}
}