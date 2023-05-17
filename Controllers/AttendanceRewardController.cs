using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceRewardController : ControllerBase
{
	private readonly IAttendanceRewardService _attendanceRewardService;
	private readonly IMailService _mailService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<AttendanceRewardController> _logger;

	public AttendanceRewardController(ILogger<AttendanceRewardController> logger, MasterDataManager masterDataManager,
		IAttendanceRewardService attendanceRewardService, IMailService mailService)
	{
		_attendanceRewardService = attendanceRewardService;
		_mailService = mailService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceRewardResponse> Post(AttendanceRewardRequest request)
	{
		var authUserData = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
		var response = new AttendanceRewardResponse();

		var gameUserId = authUserData.GameUserId;

		var (errorCode, lastLoginDate, attendanceCount) = await _attendanceRewardService.UpdateLoginDateAsync(gameUserId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var reward = _masterDataManager.GetAttendanceReward(attendanceCount);

		var mail = MailGenerator.CreateAttendanceRewardMail(gameUserId, reward);

		(errorCode, var mailId )= await _mailService.InsertMailAsync(gameUserId, mail);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		errorCode = await _mailService.InsertMailItemAsync(mailId, reward.ItemCode, reward.ItemCount);
		if (errorCode != ErrorCode.None)
		{
			await _mailService.RollbackInsertMailAsync(mailId);
			await _attendanceRewardService.RollbackLoginDateAsync(gameUserId, lastLoginDate, attendanceCount);
			response.Error=errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}