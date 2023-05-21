using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceRewardController : ControllerBase
{
	private readonly IAttendanceDataCRUD _attendanceDataCRUD;
	private readonly IMailDataCRUD _mailDataCRUD;
	private readonly MasterDataProvider _masterDataProvider;
	private readonly ILogger<AttendanceRewardController> _logger;

	public AttendanceRewardController(ILogger<AttendanceRewardController> logger, MasterDataProvider masterDataProvider,
		IAttendanceDataCRUD attendanceDataCRUD, IMailDataCRUD mailDataCRUD)
	{
		_attendanceDataCRUD = attendanceDataCRUD;
		_mailDataCRUD = mailDataCRUD;
		_masterDataProvider = masterDataProvider;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceRewardResponse> Post(AttendanceRewardRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new AttendanceRewardResponse();

		var gameUserId = userAuthAndState.GameUserId;

		var (errorCode, lastLoginDate, attendanceCount) = await _attendanceDataCRUD.UpdateLoginDateAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var reward = _masterDataProvider.GetAttendanceReward(attendanceCount);

		var mail = MailGenerator.CreateAttendanceRewardMail(gameUserId, reward);

		(errorCode, var mailId )= await _mailDataCRUD.InsertMailAsync(gameUserId, mail);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		errorCode = await _mailDataCRUD.InsertMailItemAsync(mailId, reward.ItemCode, reward.ItemCount);
		if (errorCode != ErrorCode.None)
		{
			await _mailDataCRUD.RollbackInsertMailAsync(mailId);
			await _attendanceDataCRUD.RollbackLoginDateAsync(gameUserId, lastLoginDate, attendanceCount);
			response.Error=errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}