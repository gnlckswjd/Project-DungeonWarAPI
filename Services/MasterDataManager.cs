using DungeonWarAPI.Models.DAO.Game;
using DungeonWarAPI.Models.Database.Game;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using StackExchange.Redis;
using ZLogger;

namespace DungeonWarAPI.Services;

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

    public AttendanceReward GetAttendanceReward(Int16 attendanceCount)
    {
	    return AttendanceRewardList[attendanceCount - 1];
    }

    public List<PackageItem> GetPackageItems(Int32 packageId)
    {
	    return PackageItemList.FindAll(packageItem => packageItem.PackageId == packageId);
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

		
		Versions = new Versions{ AppVersion = "1.0.0", MasterDataVersion = "1.0.0"};
		return ErrorCode.None;
	}

    private void ValidateErrorCode(ErrorCode errorCode)
    {
	    if (errorCode != ErrorCode.None)
	    {
			_logger.ZLogErrorWithPayload(new{ErrorCode=errorCode},"MasterDataLoadFail");
		    return ;
	    }
	}
}