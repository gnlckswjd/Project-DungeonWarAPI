using DungeonWarAPI.Enum;

namespace DungeonWarAPI.Models.DTO.RequestResponse;

public class ReceivePurchasedInAppItemRequest
{
    public string ReceiptSerialCode { get; set; }

    public int PackageId { get; set; }

}

public class ReceivePurchasedInAppItemResponse
{
    public ErrorCode Error { get; set; }
}