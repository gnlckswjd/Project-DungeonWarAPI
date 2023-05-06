namespace DungeonWarAPI.Models.DTO;

public class InAppRequest
{
	public String ReceiptSerialCode { get; set; }

	public Int32 PackageId { get; set; }

}

public class InAppResponse
{
	public ErrorCode Error { get; set; }
}