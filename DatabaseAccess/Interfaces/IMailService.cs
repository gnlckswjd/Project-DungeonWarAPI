using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.Database.Game;
using DungeonWarAPI.Models.DTO.Payloads;

namespace DungeonWarAPI.DatabaseAccess.Interfaces;

public interface IMailService
{
    public Task<(ErrorCode, List<Mail>)> LoadMailListAsync(Int32 gameUserId, Int32 pageNumber);
    public Task<(ErrorCode, String content)> ReadMailAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> MarkMailAsReceiveAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId);
    public Task<(ErrorCode, Int64 mailId)> InsertMailAsync(Int32 gameUserId, Mail mail);
    public Task<ErrorCode> RollbackInsertMailAsync(Int64 mailId);
    public Task<(ErrorCode errorCode, List<MailItem> items)> LoadMailItemsAsync(Int32 gameUserId, Int64 mailId);

	public Task<ErrorCode> InsertMailItemAsync(Int64 mailId, Int32 itemCode, Int32 itemCount);
}