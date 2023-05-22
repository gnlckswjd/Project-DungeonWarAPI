using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ReceiveAttendanceRewardController : ControllerBase
{
	private readonly IAttendanceDataCRUD _attendanceDataCRUD;
	private readonly IMailDataCRUD _mailDataCRUD;
	private readonly MasterDataProvider _masterDataProvider;
	private readonly ILogger<ReceiveAttendanceRewardController> _logger;

	public ReceiveAttendanceRewardController(ILogger<ReceiveAttendanceRewardController> logger, MasterDataProvider masterDataProvider,
		IAttendanceDataCRUD attendanceDataCRUD, IMailDataCRUD mailDataCRUD)
	{
		_attendanceDataCRUD = attendanceDataCRUD;
		_mailDataCRUD = mailDataCRUD;
		_masterDataProvider = masterDataProvider;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ReceiveAttendanceRewardResponse> Post(ReceiveAttendanceRewardRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new ReceiveAttendanceRewardResponse();

		if (authenticatedUserState == null)
		{
			response.Error = ErrorCode.WrongAuthenticatedUserState;
			return response;
		}

		var gameUserId = authenticatedUserState.GameUserId;

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

		_logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, AttendanceCount= attendanceCount},
			"ReceiveAttendanceReward Success");

		response.Error = ErrorCode.None;
		return response;
	}
}