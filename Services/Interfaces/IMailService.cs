using DungeonWarAPI.Models.DTO;

namespace DungeonWarAPI.Services.Interfaces;

public interface IMailService
{
    public Task<(ErrorCode, List<MailWithItems>)> LoadMailListAsync(int gameUserId, int pageNumber);
    public Task<ErrorCode> MarkMailAsReadAsync(int gameUserId, long mailId);
    public Task<ErrorCode> MarkMailItemAsReceiveAsync(int gameUserId, long mailId);
    public Task<ErrorCode> RollbackMarkMailItemAsReceiveAsync(int gameUserId, long mailId);
    public Task<ErrorCode> ReceiveItemAsync(int gameUserId, long mailId);
    public Task<ErrorCode> DeleteMailAsync(int gameUserId, long mailId);
}