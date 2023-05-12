namespace DungeonWarAPI.Enum;

public enum ErrorCode : int
{
    None = 0,

    LoadAttendanceRewardFailException = 10,
    LoadAttendanceRewardsFailSelect = 11,
    LoadItemsFailException = 20,
    LoadItemsFailSelect = 21,
    LoadItemAttributeFailException = 30,
    LoadItemAttributeFailSelect = 31,
    LoadPackageItemFailException = 40,
    LoadPackageItemFailSelect = 41,
    LoadShopPackagesFailException = 50,
    LoadShopPackagesFailSelect = 51,
    LoadStageItemsFailException = 60,
    LoadStageItemsFailSelect = 61,
    LoadStageNpcsFailException = 70,
    LoadStageNpcsFailSelect = 71,

    CreateAccountFailException = 110,
    CreateAccountFailDuplicate = 111,
    CreateAccountFailInsert = 112,
    CreateUserStageFailException = 115,
    CreateUserStageFailInsert = 116, 
    RollbackCreateUserStageFailException = 117,
    RollbackCreateUserStageFailDelete = 118,



	CreateUserFailException = 120,
    CreateUserFailDuplicate = 121,
    CreateUserFailInsert = 122,
    RollbackAccountFailException = 126,
    RollbackAccountFailDelete = 127,
    CreateUserAttendanceFailException = 130,
    CreateUserAttendanceFailInsert = 131,

    RollbackCreateUserAttendanceFailException = 135,
    RollbackCreateUserAttendanceFailDelete = 136,



    CreateUserItemFailException = 140,
    CreateUserItemFailInsert = 141,
    RollbackCreateUserDataFailException = 146,
    RollbackCreateUserDataFailDelete = 147,

    RollbackMarkMailItemAsReceiveFailException = 150,
    RollbackMarkMailItemAsReceiveFailUpdate = 151,

    AddNewItemAndGetIdFailException = 200,

    LoginFailException = 220,
    LoginFailNotUser = 221,
    LoginFailWrongPassword = 222,
    LoginFailUserNotExist = 223,
    RegisterUserFailSet = 224,
    RegisterUserFailException = 225,

    LoadUserDataFailException = 226,
    LoadUserDataFailSelect = 227,

    LoadUserItemsFailException = 230,
    LoadUserItemsFailSelect = 231,

    LoadNotificationsFailException = 232,
    LoadNotificationsZeroNotification = 233,

    InitializeStageDataFailException =235,
    InitializeStageDataFailDelete =236,
    IncrementItemFailException=237,
    IncrementItemFailIncrease=238,
    IncrementItemFailNoExist=239,
	IncrementNpcKillCountFailException = 240,
	IncrementNpcKillCountFailIncrease = 241,
	IncrementNpcKillCountFailNoExist = 242,
    LoadStageDataFailException=243,
    LoadStageDataFailGet=244,
    WrongStageLevel=250,



	LoadMailListFailException = 260,
    LoadMailListEmptyMail = 261,
    LoadMailListWrongPage = 262,

    StoreUserMailPageFailException = 270,
    StoreUserMailPageFailSet = 271,

    VerifyMailOwnerIdFailException = 280,
    VerifyMailOwnerIdFailWrongId = 281,
    ReadMailFailSelect = 285,
    ReadMailFailExceptions = 286,
    ReadMailFailWrongUser = 287,
    ReadMailFailUpdate = 288,

    MarkMailAsReceiveException = 290,
    MarkMailAsReceiveFailUpdate = 291,
    MarkMailAsReceiveFailSelect = 292,
    MarkMailAsReceiveFailAlreadyReceived = 293,
    MarkMailAsReceiveFailWrongGameUserId = 294,

    ReceiveItemFailException = 300,
    ReceiveItemFailInsert = 301,
    ReceiveItemFailMailHaveNoItem = 302,
    GetMailItemsFailException = 303,
    IncreaseGoldFailUpdate = 304,
    IncreasePotionFailUpdateOrInsert = 305,
    InsertOwnedItemFailInsert = 306,
    RollbackIncreaseGoldFail = 307,
    RollbackIncreasePotionFail = 308,
    RollbackInsertOwnedItemFail = 309,

    DeleteMailFailException = 310,
    DeleteMailFailDelete = 311,
    DeleteMailFail = 311,

    LoadAttendanceCountFailException = 315,
    LoadAttendanceCountFailSelect = 316,

    UpdateLoginDateFailException = 320,
    UpdateLoginDateFailUserNotFound = 321,
    UpdateLoginDateFailUpdate = 322,
    UpdateLoginDateFailAlreadyReceived = 323,

    CreateAttendanceMailFailException = 330,
    CreateAttendanceMailFailInsertMail = 331,
    CreateAttendanceMailFailInsertItem = 332,

    RollbackCreateMailFailException = 335,
    RollbackCreateMailFailDelete = 336,

    RollbackLoginDateFailException = 340,
    RollbackLoginDateFailUpdate = 341,

    StoreReceiptFailException = 350,
    StoreReceiptFailDuplicatedReceipt = 351,
    StoreReceiptFailInsert = 352,

    CreateInAppMailFailException = 355,
    CreateInAppMailFailInsertMail = 356,
    CreateInAppMailFailInsertItem = 357,

    RollbackStoreReceiptFailException = 360,
    RollbackStoreReceiptFailWrongId = 361,
    RollbackStoreReceiptFailDelete = 362,

    LoadItemFailException = 365,
    LoadItemFailSelect = 366,
    LoadItemFailWrongGameUser = 367,
    LoadItemFailisDestroyed = 367,

    ValidateEnoughGoldFailException = 370,
    ValidateEnoughGoldFailSelect = 371,
    ValidateEnoughGoldFailNotEnoughGold = 372,

    CanNotEnhancement = 375,
    WrongItemCode = 376,

    UpdateGoldFailException = 380,
    UpdateGoldFailIncrease = 381,

    UpdateEnhancementCountFailException = 390,
    UpdateEnhancementCountFailUpdate = 391,

    InsertEnhancementHistoryFailException = 400,
    InsertEnhancementHistoryFailInsert = 401,

    RollbackUpdateEnhancementCountFailException = 410,
    RollbackUpdateEnhancementCountFailUpdate = 411,

    DestroyItemFailException = 420,
    DestroyItemFailUpdate = 421,
    RollbackDestroyItemFailException = 425,
    RollbackDestroyItemFailUpdate = 426,

    LoadUserStageFailException= 500,
    LoadUserStageFailSelect= 501,
	CheckStageAccessibilityFailExceedStageLevel=502,
	ReceiveRewardItemFailException = 505,
	ReceiveRewardItemFailInsert = 506,
    UpdateExpFailException=510,
    UpdateExpFailSelect=511,
    UpdateExpFailUpdate=512,
    RollbackUpdateFailException=513,
    RollbackUpdateFailUpdate=514,


	InvalidRequestHttpBody = 1000,
    WrongAppVersion = 1001,
    WrongMasterDataVersion = 1002,
    WrongAuthTokenRequest = 1003,

    LoadAuthUserDataFailException = 1010,
    LoadAuthUserDataFailEmpty = 1011,

    LockUserRequestFailExceptions = 1020,
    LockUserRequestFailISet = 1021,

    UnLockUserRequestFailException = 1030,
    UnLockUserRequestFailNullKey = 1031,
    UnLockUserRequestFailDelete = 1032,





}