using System.Data;
using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.ModelConfiguration;
using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess.Implementations;

public class MasterDataLoader : IMasterDataLoader
{
    private readonly IOptions<DatabaseConfiguration> _configurationOptions;
    private readonly ILogger<MasterDataLoader> _logger;

    private readonly IDbConnection _databaseConnection;
    private readonly QueryFactory _queryFactory;

    public MasterDataLoader(ILogger<MasterDataLoader> logger, IOptions<DatabaseConfiguration> configurationOptions)
    {
        _configurationOptions = configurationOptions;
        _logger = logger;

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

    // 다음 Load 함수들의 기능 실패 시 로그는 호출되는 부분에서 처리
    public async Task<(ErrorCode, List<AttendanceReward>)> LoadAttendanceRewardsAsync()
    {
        _logger.ZLogDebugWithPayload(new{Data = "AttendanceReward" },"LoadAttendanceRewards Start");

        try
        {
            var attendanceRewards = await _queryFactory.Query("attendance_reward")
                .GetAsync<AttendanceReward>();

            if (attendanceRewards == null)
            {
                return (ErrorCode.LoadAttendanceRewardsFailSelect, new List<AttendanceReward>());
            }

            return (ErrorCode.None, attendanceRewards.ToList());
        }
        catch (Exception e)
        {
            return (ErrorCode.LoadAttendanceRewardsFailException, new List<AttendanceReward>());
        }

    }

    public async Task<(ErrorCode, List<Item>)> LoadItemsAsync()
    {
	    _logger.ZLogDebugWithPayload(new { Data = "Item" }, "LoadItems Start");

		try
        {
            var items = await _queryFactory.Query("item")
                .GetAsync<Item>();

            if (items == null)
            {
                return (ErrorCode.LoadItemsFailSelect, new List<Item>());
            }

            return (ErrorCode.None, items.ToList());
        }
        catch (Exception e)
        {
            return (ErrorCode.LoadItemsFailException, new List<Item>());

        }
    }

    public async Task<(ErrorCode, List<ItemAttribute>)> LoadItemAttributesAsync()
    {
	    _logger.ZLogDebugWithPayload(new { Data = "Attribute" }, "LoadItemAttributes Start");

		try
        {
            var itemAttributes = await _queryFactory.Query("item_attribute")
                .GetAsync<ItemAttribute>();

            if (itemAttributes == null)
            {
                return (ErrorCode.LoadItemAttributeFailSelect, new List<ItemAttribute>());
            }

            return (ErrorCode.None, itemAttributes.ToList());
        }
        catch (Exception e)
        {
            return (ErrorCode.LoadItemAttributeFailException, new List<ItemAttribute>());

        }
    }




    public async Task<(ErrorCode, List<PackageItem>)> LoadPackageItemsAsync()
    {
	    _logger.ZLogDebugWithPayload(new { Data = "PackageItem" }, "LoadPackageItems Start");

		try
        {
            var packageItems = await _queryFactory.Query("package_item")
                .GetAsync<PackageItem>();

            if (packageItems == null)
            {
                return (ErrorCode.LoadPackageItemFailSelect, new List<PackageItem>());
            }

            return (ErrorCode.None, packageItems.ToList());
        }
        catch (Exception e)
        {

            return (ErrorCode.LoadPackageItemFailException, new List<PackageItem>());
        }
    }


    public async Task<(ErrorCode, List<StageItem>)> LoadStageItemsAsync()
    {
	    _logger.ZLogDebugWithPayload(new { Data = "StageItem" }, "LoadStageItems Start");

		try
        {
            var stageItems = await _queryFactory.Query("stage_item")
                .GetAsync<StageItem>();

            if (stageItems == null)
            {
                return (ErrorCode.LoadStageItemsFailSelect, new List<StageItem>());
            }

            return (ErrorCode.None, stageItems.ToList());
        }
        catch (Exception e)
        {

            return (ErrorCode.LoadStageItemsFailException, new List<StageItem>());
        }
    }

    public async Task<(ErrorCode, List<StageNpc>)> LoadStageNpcsAsync()
    {
	    _logger.ZLogDebugWithPayload(new { Data = "StageNpc" }, "LoadStageNpcs Start");

		try
        {
            var stageNpcs = await _queryFactory.Query("stage_npc")
                .GetAsync<StageNpc>();

            if (stageNpcs == null)
            {
                return (ErrorCode.LoadStageNpcsFailSelect, new List<StageNpc>());
            }

            return (ErrorCode.None, stageNpcs.ToList());
        }
        catch (Exception e)
        {
            return (ErrorCode.LoadStageNpcsFailException, new List<StageNpc>());

        }
    }

}