using DungeonWarAPI.DatabaseAccess;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO.RequestResponse;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;

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
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new ItemAcquisitionResponse();
		var gameUserId = authUserData.GameUserId;

		var itemCode = request.ItemCode;

		var key = MemoryDatabaseKeyGenerator.MakeStageKey(request.Email);

		var (errorCode, itemAcquisitionCount, maxItemCount) = await LoadAcquisitionAndMaxItemCountAsync(key, itemCode);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		Int32 requestAcquisitionCount= request.ItemCount;

		if (itemCode != (Int32)ItemCode.Gold && itemCode != (Int32)ItemCode.Potion)
		{
			requestAcquisitionCount = 1;
		}

		if (itemAcquisitionCount + requestAcquisitionCount > maxItemCount)
		{
			response.Error = ErrorCode.ExceedItemCount;
			return response;
		}

		errorCode = await _memoryDatabase.IncrementItemCountAsync(key, itemCode, requestAcquisitionCount);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}

	private async Task<(ErrorCode, Int32 itemAcquisitionCount, Int32 maxItemSpawnCount)> LoadAcquisitionAndMaxItemCountAsync(String key, Int32 itemCode)
	{
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

		(errorCode, var itemAcquisitionCount) = await _memoryDatabase.LoadItemAcquisitionCountAsync(key, itemCode);
		if (errorCode != ErrorCode.None)
		{
			return (errorCode, 0, 0);
		}

		return (ErrorCode.None, itemAcquisitionCount, stageItem.ItemCount);
	}
}