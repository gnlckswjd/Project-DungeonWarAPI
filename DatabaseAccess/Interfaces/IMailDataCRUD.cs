using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.Payloads;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IMailDataCRUD
{
	public Task<(ErrorCode, Int64 mailId)> InsertMailAsync(Int32 gameUserId, Mail mail);
	public Task<ErrorCode> InsertMailItemAsync(Int64 mailId, Int32 itemCode, Int32 itemCount);
	public Task<ErrorCode> CreateInAppMailAsync(Int32 gameUserId, List<PackageItem> packageItems);

	public Task<(ErrorCode, List<Mail>)> LoadMailListAsync(Int32 gameUserId, Int32 pageNumber);
    public Task<(ErrorCode errorCode, List<MailItem> items)> LoadMailItemsAsync(Int32 gameUserId, Int64 mailId);
	public Task<(ErrorCode, String content)> ReadMailAsync(Int32 gameUserId, Int64 mailId);

    public Task<ErrorCode> UpdateMailStatusToReceivedAsync(Int32 gameUserId, Int64 mailId);

    public Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId);

	public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
	public Task<ErrorCode> RollbackInsertMailAsync(Int64 mailId);


}