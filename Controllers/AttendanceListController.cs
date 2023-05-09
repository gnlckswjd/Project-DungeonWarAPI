using DungeonWarAPI.Services.Interfaces;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceListController : ControllerBase
{
	private readonly IAttendanceRewardService _attendanceRewardService;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<AttendanceListController> _logger;

	public AttendanceListController(ILogger<AttendanceListController> logger, MasterDataManager masterDataManager,
		IAttendanceRewardService attendanceRewardService)
	{
		_attendanceRewardService = attendanceRewardService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceListResponse> Post(AttendanceListRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new AttendanceListResponse();

		var gameUserId = authUserData.GameUserId;

		var (errorCode, attendanceCount) = await _attendanceRewardService.LoadAttendanceCountAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.AttendanceCount= attendanceCount;
		response.Error = ErrorCode.None;
		return response;
	}

}