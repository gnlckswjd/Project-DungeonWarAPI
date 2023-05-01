namespace DungeonWarAPI;

public enum ErrorCode : int
{
	None=0,

	LoadAttendanceRewardFailException =100,
	LoadAttendanceRewardsFailSelect =101,
	LoadItemsFailException =200,
	LoadItemsFailSelect = 201,
	LoadItemAttributeFailException =300,
	LoadItemAttributeFailSelect = 301,
	LoadPackageItemFailException =400,
	LoadPackageItemFailSelect = 401,
	LoadShopPackagesFailException =500,
	LoadShopPackagesFailSelect = 501,
	LoadStageItemsFailException =600,
	LoadStageItemsFailSelect = 601,
	LoadStageNpcsFailException =700,
	LoadStageNpcsFailSelect = 701,

	CreateAccountFailException = 1010,
	CreateAccountFailDuplicate =1011,
	CreateAccountFailInsert =1012,

	CreateUserFailException=1020,
	CreateUserFailDuplicate =1021,
	CreateUserFailInsert = 1022,
	RollbackAccountFailException=1026,
	RollbackAccountFailDelete=1027,

	CreateUserItemFailException=1040,
	CreateUserItemFailInsert = 1041,
	RollbackUserDataFailException =1046,
	RollbackUserDataFailDelete=1047,

	LoginFailException = 2020,
	LoginFailNotUser = 2021,
	LoginFailWrongPassword = 2022,
	LoginFailUserNotExist =2023,
	LoginFailRegisterToRedis = 2024,
	LoginFailRegisterToRedisException =2025,

	LoadUserDataFailException = 2030,
	LoadUserDataFailSelect= 2031,

	LoadUserItemsFailException=2036,
	LoadUserItemsFailSelect=2037,

	LoadNotificationsFailException =2050,
	LoadNotificationsZeroNotification=2051,

	LoadMailsFailException =2060,
	LoadMailsFailSelect=2061,

	StoreUserMailPageFailException= 2070,
	StoreUserMailPageFailSet=2071,

	VerifyMailOwnerIdFailException = 2080,
	VerifyMailOwnerIdFailWrongId= 2081,
	MarkMailAsReadFailUpdate=2085,
	MarkMailAsReadFailExceptions = 2086,

	InvalidRequestHttpBody =3020,
	WrongAppVersion=3021,
	WrongMasterDataVersion=3022,
	WrongAuthTokenRequest=3023,

	LoadAuthUserDataFailException = 3030,
	LoadAuthUserDataFailEmpty = 3031,


	LockUserRequestFailExceptions=3040,
	LockUserRequestFailISet=3041,

	UnLockUserRequestFailException=3050,
	UnLockUserRequestFailNullKey=3051,
	UnLockUserRequestFailDelete=3052,





}