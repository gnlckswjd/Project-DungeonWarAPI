using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DAO.Account;

public class UserAuthAndState
{
    public String Email { get; set; }
    public String AuthToken { get; set; }
    public Int32 GameUserId { get; set; }
    public Int32 PlayerId { get; set; }

    public UserStateCode State { get; set; }

}