using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Services.Interfaces;

public interface IDungeonStageService
{

	public Task<(ErrorCode, Int32)> LoadStageList(Int32 gameUserId);

	public Task<ErrorCode> CheckStageAccessibility(Int32 gameUserId, Int32 selectedStageLevel);
	//public Task<ErrorCode> StartStage(Int32 gameUserId, Int32 stageId);
	//public Task<ErrorCode> ReportItemFound(Int32 gameUserId, Int32 itemId);
	//public Task<ErrorCode> ReportNpcDefeated(Int32 gameUserId, Int32 npcId);
	//public Task<ErrorCode> ReportStageCompleted(Int32 gameUserId, Int32 stageId);
	//public Task<ErrorCode> ReportStageAborted(Int32 gameUserId, Int32 stageId);
}