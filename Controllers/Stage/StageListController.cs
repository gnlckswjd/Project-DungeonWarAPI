﻿using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Stage;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers.Stage;

[Route("[controller]")]
[ApiController]
public class StageListController : ControllerBase
{
    private readonly IDungeonStageService _dungeonStageService;
    private readonly MasterDataManager _masterDataManager;
    private readonly ILogger<StageListController> _logger;

    public StageListController(ILogger<StageListController> logger, MasterDataManager masterDataManager,
        IDungeonStageService dungeonStageService)
    {
        _dungeonStageService = dungeonStageService;
        _masterDataManager = masterDataManager;
        _logger = logger;
    }

    [HttpPost]
    public async Task<StageListResponse> Post(StageListRequest request)
    {
        var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
        var response = new StageListResponse();
        var gameUserId = userAuthAndState.GameUserId;

        var (errorCode, maxClearedStage) = await _dungeonStageService.LoadStageListAsync(gameUserId);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        response.MaxClearedStage = maxClearedStage;
        response.Error = ErrorCode.None;
        return response;
    }

}