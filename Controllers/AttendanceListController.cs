using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;

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
		var authUserData = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new AttendanceListResponse();

		var gameUserId = authUserData.GameUserId;
		// 리스트 확인할 때 Date 확인
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