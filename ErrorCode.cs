namespace firstAPI;

public enum ErrorCode : int
{
	None=0,

	CreateAccountFailDuplicate =11,
	CreateAccountFailException = 12,
	LoginFailNotUser = 21,
	LoginFailWrongPassword = 22,

}