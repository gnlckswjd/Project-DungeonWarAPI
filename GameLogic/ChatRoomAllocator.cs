namespace DungeonWarAPI.GameLogic;

public class ChatRoomAllocator
{
	private UInt32 _currentRoomNumber = 0;

	public UInt32 AllocateRoomNumber()
	{
		var roomNumber = Interlocked.Increment(ref _currentRoomNumber) % 100;
		return roomNumber;
	}
}