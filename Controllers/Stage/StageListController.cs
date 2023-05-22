using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers.Stage;

[Route("[controller]")]
[ApiController]
public class StageListController : ControllerBase
{
    private readonly IStageDataCRUD _stageDataCRUD;
    private readonly ILogger<StageListController> _logger;

    public StageListController(ILogger<StageListController> logger, IStageDataCRUD stageDataCRUD)
    {
        _stageDataCRUD = stageDataCRUD;
        _logger = logger;
    }

    [HttpPost]
    public async Task<StageListResponse> Post(StageListRequest request)
    {
        var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new StageListResponse();

        if (authenticatedUserState == null)
        {
	        response.Error = ErrorCode.WrongAuthenticatedUserState;
	        return response;
        }

		var gameUserId = authenticatedUserState.GameUserId;

        var (errorCode, maxClearedStage) = await _stageDataCRUD.LoadStageListAsync(gameUserId);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, MaxClearedStageLevel = maxClearedStage },
	        "StageList Success");

		response.MaxClearedStage = maxClearedStage;
        response.Error = ErrorCode.None;
        return response;
    }

}