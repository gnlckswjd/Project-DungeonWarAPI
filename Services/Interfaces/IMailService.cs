using DungeonWarAPI.Models.DTO;

namespace DungeonWarAPI.Services.Interfaces;

public interface IMailService
{
    public Task<(ErrorCode, List<MailWithItems>)> LoadMailListAsync(Int32 gameUserId, Int32 pageNumber);
    public Task<ErrorCode> MarkMailAsReadAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> MarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> ReceiveItemAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId);
}