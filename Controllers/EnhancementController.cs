using DungeonWarAPI.Models.DAO.Account;
using DungeonWarAPI.Models.DTO;
using DungeonWarAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers;

[Route("[controller]")]
[ApiController]

public class EnhancementController : ControllerBase
{

	private readonly IGameDatabase _gameDatabase;
	private readonly MasterDataManager _masterDataManager;
	private readonly ILogger<EnhancementController> _logger;

	public EnhancementController(ILogger<EnhancementController> logger, MasterDataManager masterDataManager,
		IGameDatabase gameDatabase)
	{
		_gameDatabase = gameDatabase;
		_masterDataManager = masterDataManager;
		_logger = logger;
	}

	[HttpPost]
	public async Task<EnhancementResponse> Post(EnhancementRequest request)
	{
		var authUserData = HttpContext.Items[nameof(AuthUserData)] as AuthUserData;
		var response = new EnhancementResponse();

		var gameUserId = authUserData.GameUserId;

		var (errorCode, itemCode, enhancementCount) = await _gameDatabase.LoadItemAsync(gameUserId, request.ItemId);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}

		// 강화 가능 여부 및 최대 강화 수치 확인
		var (maxCount, gold) =  _masterDataManager.GetEnhanceMaxCount(itemCode);
		if (maxCount == -1)
		{
			response.Error = ErrorCode.WrongItemCode;
			return response;

		}
		else if (maxCount == 0)
		{
			response.Error = ErrorCode.CanNotEnhancement;
			return response;
		}

		// 강화 시도 및 성공 여부 결정
		Boolean isSuccess = ItemEnhancementManager.EnhanceItem();


		// 골드 수치 조정
		errorCode= await _gameDatabase.UpdateGoldAsync(gameUserId, gold);
		if (errorCode != ErrorCode.None)
		{
			response.Error = errorCode;
			return response;
		}
		// 아이템 업그레이드 수치 조정 및 롤백 처리
		if (isSuccess)
		{
			errorCode = await _gameDatabase.UpdateEnhancementCountAsync(gameUserId, request.ItemId, enhancementCount);
			if (errorCode != ErrorCode.None)
			{
				_gameDatabase.RollbackUpdateMoneyAsync(gameUserId, gold);
				response.Error = errorCode;
				return response;
			}
		}

		// 강화 이력 데이터 입력 및 롤백 처리
		errorCode = await _gameDatabase.InsertEnhancementHistoryAsync(gameUserId, request.ItemId, enhancementCount,
			isSuccess);
		if (errorCode != ErrorCode.None)
		{
			if (isSuccess)
			{
				_gameDatabase.RollbackUpdateEnhancementCountAsync(request.ItemId);
			}
			_gameDatabase.RollbackUpdateMoneyAsync(gameUserId, gold);
			response.Error = errorCode;
			return response;
		}

		response.Error = ErrorCode.None;
		return response;
	}
}