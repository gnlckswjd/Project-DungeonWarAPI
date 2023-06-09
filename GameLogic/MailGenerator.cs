﻿using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.GameLogic;

public static class MailGenerator
{
	private const String rewardMailTitle = "일 출석 보상";
	private const String rewardMailContents = "일 출석 보상 지급 안내";

	private const String inAppMailTitle = "구매 지급";
	private const String inAppMailContents = "구매 지급 안내";
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

	public static Mail CreateInAppMail(Int32 gameUserId, Int32 packageId)
	{
		return new Mail
		{
			GameUserId = gameUserId,
			// PackageId는 모두 동일하기 때문에 어느 인덱스에 접근하든 무방
			Title = packageId + inAppMailTitle,
			Contents = packageId + inAppMailContents,
			IsRead = false,
			IsReceived = false,
			IsInApp = true,
			IsRemoved = false,
		};
	}
}