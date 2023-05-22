using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using DungeonWarAPI.Models.DTO.RequestResponse;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class AttendanceListController : ControllerBase
{
	private readonly IAttendanceDataCRUD _attendanceDataCRUD;
	private readonly ILogger<AttendanceListController> _logger;

	public AttendanceListController(ILogger<AttendanceListController> logger, IAttendanceDataCRUD attendanceDataCRUD)
	{
		_attendanceDataCRUD = attendanceDataCRUD;
		_logger = logger;
	}

	[HttpPost]
	public async Task<AttendanceListResponse> Post(AttendanceListRequest request)
	{
		var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
		var response = new AttendanceListResponse();

		if (authenticatedUserState == null)
		{
			response.Error = ErrorCode.WrongAuthenticatedUserState;
			return response;
		}

		var gameUserId = authenticatedUserState.GameUserId;

		var (errorCode, attendanceCount) = await _attendanceDataCRUD.LoadAttendanceCountAsync(gameUserId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		_logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, AttendanceCount=attendanceCount },
			"AttendanceList Success");

		response.AttendanceCount= attendanceCount;
		response.Error = ErrorCode.None;
		return response;
	}

}