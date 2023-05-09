using DungeonWarAPI.Models.DTO.Payloads;

namespace DungeonWarAPI.Services.Interfaces;

public interface IMailService
{
    public Task<(ErrorCode, List<MailWithItems>)> LoadMailListAsync(Int32 gameUserId, Int32 pageNumber);
    public Task<(ErrorCode, string content)> ReadMailAsync(int gameUserId, long mailId);
    public Task<ErrorCode> MarkMailAsReceiveAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> ReceiveItemAsync(Int32 gameUserId, Int64 mailId);
    public Task<ErrorCode> DeleteMailAsync(Int32 gameUserId, Int64 mailId);
}