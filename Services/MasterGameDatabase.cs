using DungeonWarAPI.Game;
using DungeonWarAPI.ModelConfiguration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;

namespace DungeonWarAPI.Services;

public class MasterGameDatabase : IMasterDatabase
{
    private readonly IOptions<DatabaseConfiguration> _configurationOptions;
    private readonly ILogger<GameDatabase> _logger;

    private readonly IDbConnection _databaseConnection;
    private readonly QueryFactory _queryFactory;

    public MasterGameDatabase(ILogger<GameDatabase> logger, IOptions<DatabaseConfiguration> configurationOptions)
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

    public async Task<(ErrorCode, List<AttendanceReward>)> LoadAttendanceRewardsAsync()
    {

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
            return (ErrorCode.LoadAttendanceRewardsFailSelect, new List<AttendanceReward>());
        }

    }

    public async Task<(ErrorCode, List<Item>)> LoadItemsAsync()
    {
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
        try
        {
            var packageItems = await _queryFactory.Query("item_attribute")
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

    public async Task<(ErrorCode, List<ShopPackage>)> LoadShopPackagesAsync()
    {
        try
        {
            var shopPackages = await _queryFactory.Query("item_attribute")
                .GetAsync<ShopPackage>();

            if (shopPackages == null)
            {
                return (ErrorCode.LoadShopPackagesFailSelect, new List<ShopPackage>());
            }

            return (ErrorCode.None, shopPackages.ToList());
        }
        catch (Exception e)
        {

            return (ErrorCode.LoadShopPackagesFailException, new List<ShopPackage>());
        }
    }

    public async Task<(ErrorCode, List<StageItem>)> LoadStageItemsAsync()
    {
        try
        {
            var stageItems = await _queryFactory.Query("item_attribute")
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
        try
        {
            var stageNpcs = await _queryFactory.Query("item_attribute")
                .GetAsync<StageNpc>();

            if (stageNpcs == null)
            {
                return (ErrorCode.LoadStageItemsFailSelect, new List<StageNpc>());
            }

            return (ErrorCode.None, stageNpcs.ToList());
        }
        catch (Exception e)
        {
            return (ErrorCode.LoadStageItemsFailException, new List<StageNpc>());

        }
    }

}