﻿using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using ZLogger;

namespace DungeonWarAPI.Managers;

public class MasterDataManager
{
    //private readonly IMasterDatabase _masterDatabase;

    private readonly ILogger<MasterDataManager> _logger;

    public List<AttendanceReward> AttendanceRewardList { get; private set; }
    public List<Item> ItemList { get; private set; }
    public List<ItemAttribute> ItemAttributeList { get; private set; }
    public List<PackageItem> PackageItemList { get; private set; }
    public List<StageItem> StageItemList { get; private set; }
    public List<StageNpc> StageNpcList { get; private set; }
    public Versions Versions { get; private set; }

    public MasterDataManager(IMasterDatabase masterDatabase, ILogger<MasterDataManager> logger)
    {
        _logger = logger;
        LoadMasterData(masterDatabase).Wait();

    }

    public AttendanceReward GetAttendanceReward(short attendanceCount)
    {
        return AttendanceRewardList[attendanceCount - 1];
    }

    public List<PackageItem> GetPackageItems(int packageId)
    {
        return PackageItemList.FindAll(packageItem => packageItem.PackageId == packageId);
    }

    public (short, int) GetEnhanceMaxCountWithGold(int itemCode)
    {
        var item = ItemList.Find(item => item.ItemCode == itemCode);
        if (item == null)
        {
            return (-1, 0);
        }

        return (item.EnhanceMaxCount, -1000);
    }


    private async Task<ErrorCode> LoadMasterData(IMasterDatabase masterDatabase)
    {
        (var errorCode, AttendanceRewardList) = await masterDatabase.LoadAttendanceRewardsAsync();
        ValidateErrorCode(errorCode);

        (errorCode, ItemList) = await masterDatabase.LoadItemsAsync();
        ValidateErrorCode(errorCode);

        (errorCode, ItemAttributeList) = await masterDatabase.LoadItemAttributesAsync();
        ValidateErrorCode(errorCode);

        (errorCode, PackageItemList) = await masterDatabase.LoadPackageItemsAsync();
        ValidateErrorCode(errorCode);


        (errorCode, StageItemList) = await masterDatabase.LoadStageItemsAsync();
        ValidateErrorCode(errorCode);

        (errorCode, StageNpcList) = await masterDatabase.LoadStageNpcsAsync();
        ValidateErrorCode(errorCode);


        Versions = new Versions { AppVersion = "1.0.0", MasterDataVersion = "1.0.0" };
        return ErrorCode.None;
    }

    private void ValidateErrorCode(ErrorCode errorCode)
    {
        if (errorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { ErrorCode = errorCode }, "MasterDataLoadFail");
            return;
        }
    }
}