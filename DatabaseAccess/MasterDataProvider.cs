using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using ZLogger;

namespace DungeonWarAPI.DatabaseAccess;

public class MasterDataProvider
{
    //private readonly IMasterDataLoader _masterDatabase;

    private readonly ILogger<MasterDataProvider> _logger;

    public List<AttendanceReward> AttendanceRewardList { get; private set; }
    public List<Item> ItemList { get; private set; }
    public List<ItemAttribute> ItemAttributeList { get; private set; }
    public List<PackageItem> PackageItemList { get; private set; }
    public List<StageItem> StageItemList { get; private set; }
    public List<StageNpc> StageNpcList { get; private set; }
    public Versions Versions { get; private set; }

    public MasterDataProvider(IMasterDataLoader masterDataLoader, ILogger<MasterDataProvider> logger)
    {
        _logger = logger;
        LoadMasterData(masterDataLoader).Wait();

    }

    public AttendanceReward GetAttendanceReward(Int16 attendanceCount)
    {
        return AttendanceRewardList[attendanceCount - 1];
    }

    public List<PackageItem> GetPackageItems(Int32 packageId)
    {
        return PackageItemList.FindAll(packageItem => packageItem.PackageId == packageId);
    }

    public Int16 GetEnhanceMaxCountWithCost(Int32 itemCode)
    {
        var item = ItemList.Find(item => item.ItemCode == itemCode);
        if (item == null)
        {
            return -1;
        }

        return item.EnhanceMaxCount;
    }

    public Item GetItem(Int32 ItemCode)
    {
	    return ItemList[ItemCode - 1];
    }

    public Int32 GetAttributeCode(Int32 itemCode)
    {
	    return ItemList[itemCode - 1].AttributeCode;
    }


    public StageNpc? GetStageNpcByStageAndCode(Int32 stageLevel, Int32 npcCode)
    {
	    return StageNpcList.FirstOrDefault(npc => npc.StageLevel == stageLevel && npc.NpcCode == npcCode);
    }

    public StageItem? GetStageItemByStageAndCode(Int32 stageLevel, Int32 itemCode)
    {
	    return StageItemList.FirstOrDefault(item => item.StageLevel == stageLevel && item.ItemCode == itemCode);
    }

	public List<StageItem> GetStageItemList(Int32 stageLevel)
    {
		return StageItemList.Where(item => item.StageLevel == stageLevel).ToList();
	}

    public List<StageNpc> GetStageNpcList(Int32 stageLevel)
    {
	    return StageNpcList.Where(npc => npc.StageLevel == stageLevel).ToList();
	}
    
	private async Task<ErrorCode> LoadMasterData(IMasterDataLoader masterDataLoader)
    {
        (var errorCode, AttendanceRewardList) = await masterDataLoader.LoadAttendanceRewardsAsync();
        VerifyErrorCode(errorCode);

        (errorCode, ItemList) = await masterDataLoader.LoadItemsAsync();
        VerifyErrorCode(errorCode);

        (errorCode, ItemAttributeList) = await masterDataLoader.LoadItemAttributesAsync();
        VerifyErrorCode(errorCode);

        (errorCode, PackageItemList) = await masterDataLoader.LoadPackageItemsAsync();
        VerifyErrorCode(errorCode);


        (errorCode, StageItemList) = await masterDataLoader.LoadStageItemsAsync();
        VerifyErrorCode(errorCode);

        (errorCode, StageNpcList) = await masterDataLoader.LoadStageNpcsAsync();
        VerifyErrorCode(errorCode);


        Versions = new Versions { AppVersion = "1.0.0", MasterDataVersion = "1.0.0" };
        return ErrorCode.None;
    }

    private void VerifyErrorCode(ErrorCode errorCode)
    {
        if (errorCode != ErrorCode.None)
        {
            _logger.ZLogErrorWithPayload(new { ErrorCode = errorCode }, "MasterDataLoadFail");
            return;
        }
    }
}