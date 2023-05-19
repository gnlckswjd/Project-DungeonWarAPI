using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DAO.Redis;

public class UserAuthAndState
{
    public string Email { get; set; }
    public string AuthToken { get; set; }
    public int GameUserId { get; set; }
    public int PlayerId { get; set; }

    public UserStateCode State { get; set; }

    public int ChannelNumber { get; set; }

}