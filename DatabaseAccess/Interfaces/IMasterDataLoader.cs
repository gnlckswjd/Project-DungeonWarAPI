﻿using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IMasterDataLoader
{
    public Task<(ErrorCode, List<AttendanceReward>)> LoadAttendanceRewardsAsync();
    public Task<(ErrorCode, List<Item>)> LoadItemsAsync();
    public Task<(ErrorCode, List<ItemAttribute>)> LoadItemAttributesAsync();
    public Task<(ErrorCode, List<PackageItem>)> LoadPackageItemsAsync();
    public Task<(ErrorCode, List<StageItem>)> LoadStageItemsAsync();
    public Task<(ErrorCode, List<StageNpc>)> LoadStageNpcsAsync();

}