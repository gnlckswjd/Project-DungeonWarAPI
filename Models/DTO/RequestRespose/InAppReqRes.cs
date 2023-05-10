using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestRespose;

public class InAppRequest
{
    public string ReceiptSerialCode { get; set; }

    public int PackageId { get; set; }

}

public class InAppResponse
{
    public ErrorCode Error { get; set; }
}