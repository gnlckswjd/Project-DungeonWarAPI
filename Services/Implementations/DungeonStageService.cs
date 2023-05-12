using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using ZLogger;

namespace DungeonWarAPI.Services.Implementations;

public class DungeonStageService : IDungeonStageService
{
	private readonly IOptions<DatabaseConfiguration> _configurationOptions;
	private readonly ILogger<DungeonStageService> _logger;
	private readonly MasterDataManager _masterData;
	private readonly OwnedItemFactory _ownedItemFactory;

	private readonly IDbConnection _databaseConnection;
	private readonly QueryFactory _queryFactory;

	public DungeonStageService(ILogger<DungeonStageService> logger,
		IOptions<DatabaseConfiguration> configurationOptions, MasterDataManager masterData,
		OwnedItemFactory ownedItemFactory)
	{
		_configurationOptions = configurationOptions;
		_logger = logger;
		_masterData = masterData;
		_ownedItemFactory=ownedItemFactory;

		_databaseConnection = new MySqlConnection(configurationOptions.Value.GameDatabase);
		_databaseConnection.Open();

		var compiler = new MySqlCompiler();
		_queryFactory = new QueryFactory(_databaseConnection, compiler);
	}


	public void Dispose()
	{
		_databaseConnection.Dispose();
		//_queryFactory.Dispose();
	}

	public async Task<(ErrorCode, Int32)> LoadStageListAsync(Int32 gameUserId)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId }, "LoadStageList Start");

		try
		{
			var userStage = await _queryFactory.Query("user_stage").Where("GameUserId", "=", gameUserId)
				.FirstOrDefaultAsync<UserStage>();

			if (userStage == null)
			{
				_logger.ZLogErrorWithPayload(
					new { ErrorCode = ErrorCode.LoadUserStageFailSelect, GameUserId = gameUserId },
					"LoadUserStageFailSelect");
				return (ErrorCode.LoadUserStageFailSelect, 0);
			}

			return (ErrorCode.None, userStage.MaxClearedStage);
		}
		catch (Exception e)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.LoadUserStageFailSelect, GameUserId = gameUserId },
				"LoadUserStageFailException");
			return (ErrorCode.LoadUserStageFailException, 0);
		}
	}

	public async Task<ErrorCode> CheckStageAccessibilityAsync(Int32 gameUserId, Int32 selectedStageLevel)
	{
		_logger.ZLogDebugWithPayload(new { GameUserId = gameUserId, SelectedStageLevel = selectedStageLevel },
			"CheckStageAccessibility Start");

		var (errorCode, maxClearedStage) = await LoadStageListAsync(gameUserId);

		if (errorCode != ErrorCode.None)
		{
			return errorCode;
		}

		if (maxClearedStage + 1 < selectedStageLevel)
		{
			_logger.ZLogErrorWithPayload(new { GameUserId = gameUserId, SelectedStageLevel = selectedStageLevel },
				"CheckStageAccessibilityFailExceedStageLevel");

			return ErrorCode.CheckStageAccessibilityFailExceedStageLevel;
		}

		return ErrorCode.None;
	}

	public async Task<ErrorCode> ReceiveRewardItemAsync(Int32 gameUserId, List<(Int32,Int32)> itemCodeList)
	{
		List<Func<Task>> rollbackActions = new List<Func<Task>>();
		try
		{
			foreach (var (itemCode, itemCount) in itemCodeList)
			{
				ErrorCode errorCode;

				if (itemCode == (int)ItemCode.Gold)
				{
					errorCode = await IncreaseGoldAsync(gameUserId, itemCount, rollbackActions);
				}
				else if (itemCode == (int)ItemCode.Potion)
				{
					errorCode = await IncreasePotionAsync(gameUserId, itemCount, rollbackActions);
				}
				else
				{
					errorCode = await InsertOwnedItemAsync(gameUserId, itemCode, itemCount,
						0,
						rollbackActions);
				}

				if (errorCode != ErrorCode.None)
				{
					await RollbackReceiveItemAsync(rollbackActions);
					_logger.ZLogErrorWithPayload(new
							{ ErrorCode = ErrorCode.ReceiveRewardItemFailInsert, GameUserId = gameUserId }
						, "ReceiveRewardItemFailInsert");
					return ErrorCode.ReceiveRewardItemFailInsert;
				}
			}


			return ErrorCode.None;
		}
		catch (Exception e)
		{
			await RollbackReceiveItemAsync(rollbackActions);
			_logger.ZLogErrorWithPayload(e,
				new { ErrorCode = ErrorCode.ReceiveRewardItemFailException, GameUserId = gameUserId }
				, "ReceiveRewardItemFailException");
			return ErrorCode.ReceiveRewardItemFailException;
		}
	}

	private async Task<ErrorCode> IncreaseGoldAsync(Int32 gameUserId, Int32 itemCount, List<Func<Task>> rollbackActions)
	{
		var count = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
			.IncrementAsync("Gold", itemCount);

		if (count != 1)
		{
			_logger.ZLogErrorWithPayload(new { ErrorCode = ErrorCode.IncreaseGoldFailUpdate, GameUserId = gameUserId },
				"IncreaseGoldFailUpdate");
			return ErrorCode.IncreaseGoldFailUpdate;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("user_data").Where("GameUserId", "=", gameUserId)
				.DecrementAsync("Gold", itemCount);
			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackIncreaseGoldFail,
						GameUserId = gameUserId,
						ItemCount = itemCount
					}, "RollbackIncreaseGoldFail");
			}
		});
		return ErrorCode.None;
	}


	private async Task<ErrorCode> IncreasePotionAsync(Int32 gameUserId, Int32 itemCount,
		List<Func<Task>> rollbackActions)
	{
		ErrorCode errorCode = ErrorCode.None;
		var count = await _queryFactory.Query("owned_item").Where("GameUserId", "=", gameUserId)
			.Where("ItemCode", "=", (Int32)ItemCode.Potion)
			.IncrementAsync("ItemCount", itemCount);

		// count == 0 InsertOwnedItemAsync에서 롤백 등록 수행
		if (count == 0)
		{
			errorCode = await InsertOwnedItemAsync(gameUserId, (int)ItemCode.Potion, itemCount,
				0, rollbackActions);
		}

		if (errorCode != ErrorCode.None)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.IncreasePotionFailUpdateOrInsert, GameUserId = gameUserId },
				"IncreasePotionFailUpdateOrInsert");
			return ErrorCode.IncreasePotionFailUpdateOrInsert;
		}

		// count == 0 일 때 아이템 생성에 관한 롤백 등록은 InsertOwnedItemAsync에서 진행
		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("owned_item")
				.Where("GameUserId", "=", gameUserId)
				.Where("ItemCode", "=", (Int32)ItemCode.Potion)
				.DecrementAsync("ItemCount", itemCount);

			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackIncreasePotionFail,
						GameUserId = gameUserId,
						ItemCount = itemCount
					}, "RollbackIncreasePotionFail");
			}
		});

		return ErrorCode.None;
	}

	private async Task<ErrorCode> InsertOwnedItemAsync(Int32 gameUserId, Int32 itemCode, Int32 itemCount,
		Int32 enhancementCount, List<Func<Task>> rollbackActions)
	{
		var itemId = await _queryFactory.Query("owned_item")
			.InsertGetIdAsync<int>(
				_ownedItemFactory.CreateOwnedItem(gameUserId, itemCode, enhancementCount, itemCount));

		if (itemId == 0)
		{
			_logger.ZLogErrorWithPayload(
				new { ErrorCode = ErrorCode.InsertOwnedItemFailInsert, GameUserId = gameUserId },
				"InsertOwnedItemFailInsert");
			return ErrorCode.InsertOwnedItemFailInsert;
		}

		rollbackActions.Add(async () =>
		{
			var rollbackCount = await _queryFactory.Query("user_data").Where("ItemId", "=", itemId)
				.DeleteAsync();

			if (rollbackCount != 1)
			{
				_logger.ZLogErrorWithPayload(
					new
					{
						ErrorCode = ErrorCode.RollbackInsertOwnedItemFail,
						GameUserId = gameUserId,
						ItemId = itemId
					}, "RollbackInsertOwnedItemFail");
			}
		});

		return ErrorCode.None;
	}

	private async Task RollbackReceiveItemAsync(List<Func<Task>> rollbackActions)
	{
		foreach (var action in rollbackActions)
		{
			await action();
		}
	}
}