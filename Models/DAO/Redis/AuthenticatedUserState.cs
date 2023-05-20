using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DAO.Redis;

public class AuthenticatedUserState
{
    public String Email { get; set; }
    public String AuthToken { get; set; }
    public Int32 GameUserId { get; set; }
    public Int32 PlayerId { get; set; }

    public UserStateCode State { get; set; }

    public Int32 ChannelNumber { get; set; }

}