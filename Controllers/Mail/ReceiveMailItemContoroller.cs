using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Mail;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers.Mail;

[Route("[controller]")]
[ApiController]
public class ReceiveMailItemController : ControllerBase
{
    private readonly IMailDataCRUD _mailDataCRUD;
    private readonly IItemDATACRUD _itemDataCRUD;
    private readonly ILogger<ReceiveMailItemController> _logger;

    public ReceiveMailItemController(ILogger<ReceiveMailItemController> logger, IMailDataCRUD mailDataCRUD, IItemDATACRUD itemDataCRUD)
    {
        _mailDataCRUD = mailDataCRUD;
        _itemDataCRUD = itemDataCRUD;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ReceiveMailItemResponse> Post(ReceiveMailItemRequest request)
    {
        var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new ReceiveMailItemResponse();
        var gameUserId = userAuthAndState.GameUserId;

        var errorCode = await _mailDataCRUD.UpdateMailStatusToReceivedAsync(gameUserId, request.MailId);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        (errorCode, var items) = await _mailDataCRUD.LoadMailItemsAsync(gameUserId, request.MailId);
        if (errorCode != ErrorCode.None)
        {
            await _mailDataCRUD.RollbackMarkMailItemAsReceiveAsync(gameUserId, request.MailId);
            response.Error = errorCode;
            return response;
        }

        errorCode = await _itemDataCRUD.InsertItemsAsync(gameUserId, items);
        if (errorCode != ErrorCode.None)
        {
            await _mailDataCRUD.RollbackMarkMailItemAsReceiveAsync(gameUserId, request.MailId);
            response.Error = errorCode;
            return response;
        }


        response.Error = errorCode;
        return response;
    }
}