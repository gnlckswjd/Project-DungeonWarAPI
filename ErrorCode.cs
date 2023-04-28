﻿namespace DungeonWarAPI;

public enum ErrorCode : int
{
	None=0,
	CreateAccountFailException = 1010,
	CreateAccountFailDuplicate =1011,
	CreateAccountFailInsert =1012,

	CreateUserFailException=1020,
	CreateUserFailDuplicate =1021,
	CreateUserFailInsert = 1022,
	RollbackAccountFailException=1030,
	RollbackAccountFailDelete=1031,

	CreateUserItemFailException=1040,
	CreateUserItemFailInsert = 1041,


	LoginFailException = 2020,
	LoginFailNotUser = 2021,
	LoginFailWrongPassword = 2022,
	LoginFailUserNotExist =2023,
	LoginFailRegisterToRedis = 2024,
	LoginFailRegisterToRedisException =2025,
	NoticeFailExceptions=2050,

	


}