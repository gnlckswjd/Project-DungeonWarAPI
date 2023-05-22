using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.Payloads;
using DungeonWarAPI.Models.DTO.RequestResponse.Mail;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers.Mail;

[Route("[controller]")]
[ApiController]
public class MailListController : ControllerBase
{
    private readonly IMailDataCRUD _mailDataCRUD;
    private readonly ILogger<MailListController> _logger;

    public MailListController(ILogger<MailListController> logger, IMailDataCRUD mailDataCRUD)
    {
        _mailDataCRUD = mailDataCRUD;
        _logger = logger;
    }

    [HttpPost]
    public async Task<MailListResponse> Post(MailListRequest request)
    {
        var authenticatedUserState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new MailListResponse();

        if (authenticatedUserState == null)
        {
	        response.Error = ErrorCode.WrongAuthenticatedUserState;
	        return response;
        }

		var gameUserId = authenticatedUserState.GameUserId;

        var (errorCode, mails) = await _mailDataCRUD.LoadMailListAsync(gameUserId, request.PageNumber);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        var mailsWithItems = new List<MailWithItems>();

        foreach (var mail in mails)
        {
            (errorCode, var items) = await _mailDataCRUD.LoadMailItemsAsync(gameUserId, mail.MailId);

            if (errorCode != ErrorCode.None)
            {
                response.Error = errorCode;
                return response;
            }

            mailsWithItems.Add(new MailWithItems(mail, items));
        }

        _logger.ZLogInformationWithPayload(new { GameUserId = gameUserId }, "ReceiveMailItem Success");

		response.MailWithItemsList = mailsWithItems;
        return response;
    }
}