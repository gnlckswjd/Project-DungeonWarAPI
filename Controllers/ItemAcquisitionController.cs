using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.GameLogic;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]
public class ItemAcquisitionController : ControllerBase
{
	private readonly IDungeonStageService _dungeonStageService;
	private readonly MasterDataManager _masterDataManager;
	private readonly IMemoryDatabase _memoryDatabase;
	private readonly ILogger<ItemAcquisitionController> _logger;

	public ItemAcquisitionController(ILogger<ItemAcquisitionController> logger, IMemoryDatabase memoryDatabase,
		MasterDataManager masterDataManager,
		IDungeonStageService dungeonStageService)
	{
		_memoryDatabase = memoryDatabase;
		_dungeonStageService = dungeonStageService;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<ItemAcquisitionResponse> Post(ItemAcquisitionRequest request)
	{
		var userAuthAndState = HttpContext.Items[nameof(UserAuthAndState)] as UserAuthAndState;
		var response = new ItemAcquisitionResponse();
		var gameUserId = userAuthAndState.GameUserId;

		var itemCode = request.ItemCode;

		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

		var (errorCode, currentItemCount, maxItemCount) =
			await LoadAcquisitionAndMaxItemCountAsync(key, itemCode, gameUserId, userAuthAndState.State);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		(errorCode, var itemCount) =
			StageRequestVerifier.VerifyItemCount(itemCode, currentItemCount, request.ItemCount, maxItemCount);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}
		
		errorCode = await _memoryDatabase.IncrementItemCountAsync(key, itemCode, itemCount);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}

	private async Task<(ErrorCode, Int32 currentItemCount, Int32 maxItemSpawnCount)> LoadAcquisitionAndMaxItemCountAsync(String key, Int32 itemCode, Int32 gameUserId, UserStateCode state)
	{
		if (state != UserStateCode.InStage)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.WrongUserState, GameUserId = gameUserId },
				"CheckStageAccessibility");
			return (ErrorCode.WrongUserState, 0, 0);
		}

		var (errorCode, stageLevel) = await _memoryDatabase.LoadStageLevelAsync(key);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, 0, 0);
		}

		var stageItem = _masterDataManager.GetStageItemByStageAndCode(stageLevel, itemCode);
		if (stageItem == null)
		{
			return (ErrorCode.WrongItemCode, 0, 0);
		}

		(errorCode, var currentItemCount) = await _memoryDatabase.LoadItemAcquisitionCountAsync(key, itemCode);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, 0, 0);
		}

		return (ErrorCode.None, currentItemCount, stageItem.ItemCount);
	}
}