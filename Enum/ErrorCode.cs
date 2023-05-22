namespace DungeonWarAPI.Enum;

public enum ErrorCode : Int32
{
	None = 0,

	//Load
	LoadAttendanceRewardsFailException = 100,
	LoadAttendanceRewardsFailSelect = 101,

	LoadItemsFailException = 102,
	LoadItemsFailSelect = 103,

	LoadItemAttributeFailException = 104,
	LoadItemAttributeFailSelect = 105,

	LoadPackageItemFailException = 106,
	LoadPackageItemFailSelect = 107,

	LoadStageItemsFailException = 108,
	LoadStageItemsFailSelect = 109,

	LoadStageNpcsFailException = 110,
	LoadStageNpcsFailSelect = 111,

	LoadStageDataFailException = 120,
	LoadStageDataFailGet = 121,

	LoadStageLevelFailException = 122,
	LoadStageLevelFailGet = 123,

	LoadNpcKillCountFailException = 124,
	LoadNpcKillCountFailGet = 125,

	LoadItemAcquisitionCountFailException = 126,
	LoadItemAcquisitionCountFailGet = 127,

	LoadUserDataFailException = 130,
	LoadUserDataFailSelect = 131,

	LoadUserItemsFailException = 132,

	LoadNotificationsFailException = 133,
	LoadNotificationsZeroNotification = 134,

	LoadUserStageFailException = 135,
	LoadUserStageFailSelect = 136,

	LoadMailListFailException = 140,
	LoadMailListEmptyMail = 141,
	LoadMailListWrongPage = 142,

	LoadMailItemsFailException = 143,

	LoadAttendanceCountFailException = 150,
	LoadAttendanceCountFailSelect = 151,

	LoadItemFailException = 160,
	LoadItemFailSelect = 161,
	LoadItemFailWrongGameUser = 162,
	LoadItemFailDestroyed = 163,

	LoadLatestChatMessageFailException = 170,
	LoadLatestChatMessageFailGet = 171,

	LoadAuthUserDataFailException = 180,
	LoadAuthUserDataFailEmpty = 181,

	//Insert
	CreateAccountFailException = 200,
	CreateAccountFailDuplicate = 201,
	CreateAccountFailInsert = 202,

	CreateUserStageFailException = 203,
	CreateUserStageFailInsert = 204,

	CreateUserFailException = 205,
	CreateUserFailDuplicate = 206,
	CreateUserFailInsert = 207,

	CreateUserAttendanceFailException = 208,
	CreateUserAttendanceFailInsert = 209,

	CreateInAppMailFailException = 210,
	CreateInAppMailFailInsertMail = 211,
	CreateInAppMailFailInsertItem = 212,

	InsertNonStackableItemsException = 220,
	InsertNonStackableItemsFailInsert = 221,

	InsertItemFailException = 222,
	InsertItemFailInsert = 223,

	InsertOwnedItemFailInsert = 224,

	InsertMailFailException = 230,
	InsertMailFailInsert = 231,

	InsertMailItemFailException = 232,
	InsertMailItemFailInsert = 233,

	InsertEnhancementHistoryFailException = 240,
	InsertEnhancementHistoryFailInsert = 241,

	InsertChatMessageFailException = 250,
	InsertChatMessageFailInsert = 251,

	InsertReceiptFailException = 260,
	InsertReceiptFailDuplicatedReceipt = 261,
	InsertReceiptFailInsert = 262,

	InsertStageDataException = 270,
	InsertStageDataFailDelete = 271,

	LoginFailException = 280,
	LoginFailWrongPassword = 281,
	LoginFailUserNotExist = 282,

	RegisterUserFailSet = 283,
	RegisterUserFailException = 284,

	//Update
	UpdateUserStateFailException = 300,
	UpdateUserStateFailSet = 301,

	UpdateLoginDateFailException = 302,
	UpdateLoginDateFailUserNotFound = 303,
	UpdateLoginDateFailUpdate = 304,
	UpdateLoginDateFailAlreadyReceived = 305,

	UpdateGoldFailException = 306,
	UpdateGoldFailIncrease = 307,

	UpdateExpFailException = 308,
	UpdateExpFailSelect = 309,
	UpdateExpFailUpdate = 310,

	UpdateMaxClearedStageFailException = 311,
	UpdateMaxClearedStageFailIncrement = 312,

	UpdateChatChannelChatChannelFailException = 313,
	UpdateChatChannelChatChannelFailUpdate = 314,

	UpdateAuthenticatedUserStateFailException = 315,
	UpdateAuthenticatedUserStateFailSet = 316,

	UpdateEnhancementCountFailException = 320,
	UpdateEnhancementCountFailUpdate = 321,

	UpdateMailStatusToReceivedException = 330,
	UpdateMailStatusToReceivedUpdate = 331,

	UpdateItemStatusToDestroyAFailException = 340,
	UpdateItemStatusToDestroyAFailUpdate = 341,

	IncrementItemFailException = 350,
	IncrementItemFailIncrease = 351,
	IncrementItemFailNoExist = 352,

	IncrementNpcKillCountFailException = 360,
	IncrementNpcKillCountFailIncrease = 361,
	IncrementNpcKillCountFailNoExist = 362,

	IncreaseGoldFailUpdate = 370,
	IncreasePotionFailUpdateOrInsert = 371,

	UpdateMailStatusToReceivedFailAlreadyReceived = 362,
	UpdateMailStatusToReceivedWrongGameUserId = 363,

	ReadMailFailSelect = 370,
	ReadMailFailExceptions = 371,
	ReadMailFailWrongUser = 372,
	ReadMailFailUpdate = 373,



	//Delete
	DeleteStageDataFail = 400,
	DeleteStageDataFailException = 401,
	DeleteStageDataFailDelete = 402,

	DeleteMailFailException = 410,
	DeleteMailFailDelete = 411,

	//Rollback
	RollbackCreateAccountFailException = 500,
	RollbackCreateAccountFailDelete = 501,

	RollbackCreateUserStageFailException = 502,
	RollbackCreateUserStageFailDelete = 503,

	RollbackCreateUserAttendanceFailException = 504,
	RollbackCreateUserAttendanceFailDelete = 505,

	RollbackCreateUserDataFailException = 506,
	RollbackCreateUserDataFailDelete = 507,

	RollbackLoginDateFailException = 508,
	RollbackLoginDateFailUpdate = 509,

	RollbackMarkMailItemAsReceiveFailException = 510,
	RollbackMarkMailItemAsReceiveFailUpdate = 511,

	RollbackInsertMailFailException = 512,
	RollbackInsertMailFailDelete = 513,
	RollbackInsertMailFailWrongMailId = 514,

	RollbackCreateMailFailException = 515,
	RollbackCreateMailFailDelete = 516,

	RollbackIncreaseGoldFail = 520,
	RollbackIncreasePotionFail = 521,
	RollbackInsertOwnedItemFail = 522,

	RollbackStoreReceiptFailException = 530,
	RollbackStoreReceiptFailWrongId = 531,
	RollbackStoreReceiptFailDelete = 532,

	RollbackUpdateEnhancementCountFailException = 540,
	RollbackUpdateEnhancementCountFailUpdate = 541,

	RollbackDestroyItemFailException = 550,
	RollbackDestroyItemFailUpdate = 551,


	RollbackUpdateExpFailException = 560,
	RollbackUpdateExpFailUpdate = 561,

	RollbackUpdateMaxClearedStageFailException = 570,
	RollbackUpdateMaxClearedStageFailDecrement = 571,


	//포함되지 않는 코드
	CanNotEnhancement = 600,

	WrongItemCode = 601,
	WrongUserState = 602,
	WrongStageLevel = 603,
	WrongNpcCode = 604,

	ExceedKillCount = 605,
	ExceedItemCount = 606,

	VerifyStageAccessibilityFailExceedStageLevel = 610,
	LoadGoldFailException = 611,
	LoadGoldFailSelect = 612,
	NotEnoughGold = 613,

	WrongAuthenticatedUserState= 911,

	//미들웨어 관련
	InvalidRequestHttpBody = 1000,
	WrongAppVersionOrMasterDataVersion = 1001,
	WrongMasterDataVersion = 1002,
	WrongAuthTokenRequest = 1003,

	LockUserRequestFailExceptions = 1020,
	LockUserRequestFailISet = 1021,

	UnLockUserRequestFailException = 1030,
	UnLockUserRequestFailNullKey = 1031,
	UnLockUserRequestFailDelete = 1032,
}