using DungeonWarAPI.Models.Database.Game;

namespace DungeonWarAPI.Services;

public interface IMasterDatabase
{
	public Task<(ErrorCode,List<AttendanceReward>)> LoadAttendanceRewardsAsync();
	public Task<(ErrorCode,List<Item>)> LoadItemsAsync();
	public Task<(ErrorCode, List<ItemAttribute>)> LoadItemAttributesAsync();
	public Task<(ErrorCode, List<PackageItem>)> LoadPackageItemsAsync();
	public Task<(ErrorCode, List<ShopPackage>)> LoadShopPackagesAsync();
	public Task<(ErrorCode, List<StageItem>) > LoadStageItemsAsync();
	public Task<(ErrorCode, List<StageNpc>) > LoadStageNpcsAsync();

}