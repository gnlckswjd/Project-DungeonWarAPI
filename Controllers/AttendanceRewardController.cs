using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceRewardController : ControllerBase
{
	private readonly ILoginRewardService _loginRewardService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<AttendanceRewardController> _logger;

	public AttendanceRewardController(ILogger<AttendanceRewardController> logger, MasterDataManager masterDataManager,
		ILoginRewardService loginRewardService)
	{
		_loginRewardService = loginRewardService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceRewardResponse> Post(AttendanceRewardRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new AttendanceRewardResponse();

		var gameUserId = authUserData.GameUserId;

		var (errorCode, lastLoginDate, attendanceCount) = await _loginRewardService.UpdateLoginDateAsync(gameUserId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var reward = _masterDataManager.GetAttendanceReward(attendanceCount);

		errorCode = await _loginRewardService.CreateAttendanceRewardMailAsync(gameUserId, reward);

		if (errorCode != ErrorCode.None)
		{
			await _loginRewardService.RollbackLoginDateAsync(gameUserId, lastLoginDate, attendanceCount);
			response.Error=errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}