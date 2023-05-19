using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse.Stage;

public class ItemAcquisitionRequest
{
    public int ItemCode { get; set; }

    public int ItemCount { get; set; }

    public string Email { get; set; }
}

public class ItemAcquisitionResponse
{
    public ErrorCode Error { get; set; }
}