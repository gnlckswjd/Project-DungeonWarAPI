using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Services;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceRewardController : ControllerBase
{
	private readonly IAttendanceRewardService _attendanceRewardService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<AttendanceRewardController> _logger;

	public AttendanceRewardController(ILogger<AttendanceRewardController> logger, MasterDataManager masterDataManager,
		IAttendanceRewardService attendanceRewardService)
	{
		_attendanceRewardService = attendanceRewardService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceRewardResponse> Post(AttendanceRewardRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new AttendanceRewardResponse();

		var gameUserId = authUserData.GameUserId;

		var (errorCode, lastLoginDate, attendanceCount) = await _attendanceRewardService.UpdateLoginDateAsync(gameUserId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var reward = _masterDataManager.GetAttendanceReward(attendanceCount);

		errorCode = await _attendanceRewardService.CreateAttendanceRewardMailAsync(gameUserId, reward);

		if (errorCode != ErrorCode.None)
		{
			await _attendanceRewardService.RollbackLoginDateAsync(gameUserId, lastLoginDate, attendanceCount);
			response.Error=errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}