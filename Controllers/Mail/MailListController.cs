using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.Payloads;
using DungeonWarAPI.Models.DTO.RequestResponse.Mail;
using Microsoft.AspNetCore.Mvc;

namespace DungeonWarAPI.Controllers.Mail;

[Route("[controller]")]
[ApiController]
public class MailListController : ControllerBase
{
    private readonly IMailService _mailService;
    private readonly ILogger<MailListController> _logger;

    public MailListController(ILogger<MailListController> logger, IMailService mailService)
    {
        _mailService = mailService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<MailListResponse> Post(MailListRequest request)
    {
        var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new MailListResponse();
        var gameUserId = userAuthAndState.GameUserId;

        var (errorCode, mails) = await _mailService.LoadMailListAsync(gameUserId, request.PageNumber);
        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        var mailsWithItems = new List<MailWithItems>();

        foreach (var mail in mails)
        {
            (errorCode, var items) = await _mailService.LoadMailItemsAsync(gameUserId, mail.MailId);

            if (errorCode != ErrorCode.None)
            {
                response.Error = errorCode;
                return response;
            }

            mailsWithItems.Add(new MailWithItems(mail, items));
        }

        response.MailWithItemsList = mailsWithItems;
        return response;
    }
}