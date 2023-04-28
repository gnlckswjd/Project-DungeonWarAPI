namespace DungeonWarAPI.ModelDatabase;

public class Inventory
{
	public Byte[] AccountId { get; set; }
	public Int32 ItemCode { get; set; }
	public Int32 EnhancementValue { get; set; }
	public Int32 ItemCount { get; set; }
}