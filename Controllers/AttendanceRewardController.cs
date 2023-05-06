using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceRewardController : ControllerBase
{
	private readonly IGameDatabase _gameDatabase;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<AttendanceRewardController> _logger;

	public AttendanceRewardController(ILogger<AttendanceRewardController> logger, MasterDataManager masterDataManager,
		IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceRewardResponse> Post(AttendanceRewardRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new AttendanceRewardResponse();

		var gameUserId = authUserData.GameUserId;

		var (errorCode, lastLoginDate, attendanceCount) = await _gameDatabase.UpdateLoginDateAsync(gameUserId);

		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		var reward = _masterDataManager.GetAttendanceReward(attendanceCount);

		errorCode = await _gameDatabase.CreateAttendanceRewardMailAsync(gameUserId, reward);

		if (errorCode != ErrorCode.None)
		{
			await _gameDatabase.RollbackLoginDateAsync(gameUserId, lastLoginDate, attendanceCount);
			response.Error=errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}