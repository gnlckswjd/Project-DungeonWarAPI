namespace firstAPI;

public enum ErrorCode : int
{
	None=0,
	CreateAccountFailException = 1010,
	CreateAccountFailDuplicate =1011,
	CreateAccountFailInsert =1012,

	LoginFailException = 2020,
	LoginFailNotUser = 2021,
	LoginFailWrongPassword = 2022,
	LoginFailUserNotExist =2023,
	LoginFailRegisterToRedis = 2024,
	LoginFailRegisterToRedisException =2025,
	NoticeFailExceptions=2050,

	


}