using DungeonWarAPI.DatabaseAccess.Interfaces;
using DungeonWarAPI.Enum;
using DungeonWarAPI.Models.DAO.Redis;
using DungeonWarAPI.Models.DTO.RequestResponse.Mail;
using Microsoft.AspNetCore.Mvc;
using ZLogger;

namespace DungeonWarAPI.Controllers.Mail;

[Route("[controller]")]
[ApiController]
public class ReadMailController : ControllerBase
{
    private readonly IMailDataCRUD _mailDataCRUD;
    private readonly ILogger<ReadMailController> _logger;

    public ReadMailController(ILogger<ReadMailController> logger, IMailDataCRUD mailDataCRUD)
    {
        _mailDataCRUD = mailDataCRUD;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ReadMailResponse> Post(ReadMailRequest request)
    {
        var userAuthAndState = HttpContext.Items[nameof(AuthenticatedUserState)] as AuthenticatedUserState;
        var response = new ReadMailResponse();

        var gameUserId = userAuthAndState.GameUserId;
        var mailId = request.MailId;

        var (errorCode, content) = await _mailDataCRUD.ReadMailAsync(gameUserId, request.MailId);

        if (errorCode != ErrorCode.None)
        {
            response.Error = errorCode;
            return response;
        }

        _logger.ZLogInformationWithPayload(new { GameUserId = gameUserId, MailId = mailId }, "ReadMail Success");

		response.Error = errorCode;
        response.Content = content;
        return response;
    }
}