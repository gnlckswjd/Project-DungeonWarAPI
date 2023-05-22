# DungeonWarAPI-정휘찬
![Dungeon War API](https://github.com/gnlckswjd/Project-DungeonWarAPI/assets/52772732/1d72f22c-7a11-471b-960b-26c3e23f3db6)



## 프로젝트 개요
컴투스 서버캠퍼스 1기에 참여하며 만든 **웹 서버 방식의 게임서버**입니다.

### 사용 기술
- <img src="https://img.shields.io/badge/-ASP.NET%20Core%207.0-brightgreen"> 
- <img src="https://img.shields.io/badge/-MySql%208.0.31.0-yellow">

  - <img src="https://img.shields.io/badge/MySqlConnector%202.2.5-green">
  - <img src="https://img.shields.io/badge/-SqlKata%202.4.0-yellowgreen">
- <img src ="https://img.shields.io/badge/-Redis%206.0.16-red">

  - <img src ="https://img.shields.io/badge/-CloudStructures%203.2.0-blue">
- <img src ="https://img.shields.io/badge/-ZLogger%201.7.0-lightgrey">

### 개발 기간
- 프로젝트 개발 : 2023/4/25 ~
- 기술 학습 : 2023/4/18 ~ 2023/4/24
  - <img src="https://img.shields.io/badge/-ASP.NET%20Core-brightgreen">  
  - <img src="https://img.shields.io/badge/-SqlKata-yellowgreen">
  - <img src ="https://img.shields.io/badge/-CloudStructures-blue">
  - <img src ="https://img.shields.io/badge/-ZLogger-lightgrey">
  
### 개발 인원 
- 1명 (정휘찬)

## 기능
- 모든 요청은 POST 요청입니다.
- 모든 요청은 `AppVersion` 과 `MasterDataVersion`을 포함해야 합니다.
- `CreatAccount`와 `Login`을 제외한 요청은 추가로 `Email`과 `AuthToken`을 포함해야합니다.

### 계정 생성
- URI: `/CreateAccount`
```csharp
public class CreateAccountRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class CreateAccountResponse
{
    public ErrorCode Error { get; set; }
}
```

### 로그인
- URI: `/Login`
```csharp
public class LoginRequest
{
    public string Email { get; set; }

    public string Password { get; set; }
}

public class LoginResponse
{
    public ErrorCode Error { get; set; }

    public int UserLevel { get; set; }

    public List<OwnedItem> Items { get; set; }

    public string AuthToken { get; set; }

    public List<string> Notifications { get; set; }
}
```

### 출석 리스트 보기
- URI: `/AttendanceList`
```csharp
public class AttendanceListRequest
{
    //공통적으로 포함해야하는 Email과 AuthToken 외에 필요한 것 없음  
}

public class AttendanceListResponse
{
    public ErrorCode Error { get; set; }
    public Int32 AttendanceCount { get; set; }
}
```

### 출석 보상 받기
- URI: `/ReceiveAttendanceReward`
```csharp
public class ReceiveAttendanceRewardRequest
{
    //공통적으로 포함해야하는 Email과 AuthToken 외에 필요한 것 없음  
}
public class ReceiveAttendanceRewardResponse
{
    public ErrorCode Error { get; set; }

}
```

### 메일 리스트 받기
- URI: `/MailList`
```csharp
public class MailListRequest
{
    public Int32 PageNumber { get; set; }
}

public class MailListResponse
{
    public ErrorCode Error { get; set; }
    public List<MailWithItems> MailWithItemsList { get; set; }

}
```

### 메일 읽기
- URI: `/ReadMail`
```csharp

public class ReadMailRequest
{
    public Int64 MailId { get; set; }

}

public class ReadMailResponse
{
    public ErrorCode Error { get; set; }
    public String Content { get; set; }

}
```

### 메일 삭제
- URI: `/DeleteMail`
```csharp
public class DeleteMailRequest
{
    public Int32 MailId { get; set; }
}


public class DeleteMailResponse
{
    public ErrorCode Error { get; set; }
}
```

### 메일 아이템 수령
- URI: `/ReceiveMailItem`
```csharp
public class ReceiveMailItemRequest
{
    public Int64 MailId { get; set; }
}
public class ReceiveMailItemResponse
{
    public ErrorCode Error { get; set; }
}
```

### 인앱 구매 아이템 수령
- URI: `/ReceivePurchasedInAppItem`
```csharp
public class ReceivePurchasedInAppItemRequest
{
    public string ReceiptSerialCode { get; set; }

    public int PackageId { get; set; }
}

public class ReceivePurchasedInAppItemResponse
{
    public ErrorCode Error { get; set; }
}
```

### 아이템 강화
- URI: `/Enhancement`
```csharp
public class EnhancementRequest
{
    public long ItemId { get; set; }

}

public class EnhancementResponse
{
    public ErrorCode Error { get; set; }

    public bool EnhancementResult { get; set; }
}
```

### 스테이지 리스트
- URI: `/StageList`
```csharp
public class StageListRequest
{

}
public class StageListResponse
{
    public ErrorCode Error { get; set; }
    public Int32 MaxClearedStage { get; set; }
}
```

### 스테이지 시작
- URI: `/StageStart`
```csharp
public class StageStartRequest
{
    public String Email { get; set; }
    public Int32 SelectedStageLevel { get; set; }
}
public class StageStartResponse
{
    public ErrorCode Error { get; set; }
    public List<StageItem> ItemList { get; set; }
    public List<StageNpc> NpcList { get; set; }
}

```

### 아이템 획득
- URI: `ItemAcquisition`
```csharp
public class ItemAcquisitionRequest
{
    public Int32 ItemCode { get; set; }

    public Int32 ItemCount { get; set; }

    public String Email { get; set; }
}

public class ItemAcquisitionResponse
{
    public ErrorCode Error { get; set; }
}
```

### NPC 킬
- URI: `/NpcKill`
```csharp
public class NpcKillRequest
{
    public Int32 NpcCode { get; set; }

    public String Email { get; set; }
}
public class NpcKillResponse
{
    public ErrorCode Error { get; set; }
}
```

### 스테이지 종료
- URI: `/StageEnd`
```csharp
public class StageEndRequest
{
    public String Email { get; set; }
}

public class StageEndResponse
{
    public bool IsCleared { get; set; }
    public ErrorCode Error { get; set; }
}
```

### 채팅 메시지 전송
- URI: `/SendChat`
```csharp
public class SendChatRequest
{
	public String Message { get; set; }
}
public class SendChatResponse
{
	public ErrorCode Error { get; set; }
}
```

### 최근 채팅 메시지 받기
- URI: `LoadLatestChatMessage`
```csharp
public class LoadLatestChatMessageRequest
{
	public String Email { get; set; }
	public String MessageId { get; set; }
}

public class LoadLatestChatMessageResponse
{
	public ErrorCode Error { get; set; }
	public ChatMessageReceived ChatMessage { get; set; }
}
```

### 채팅 히스토리 받기
- URI: `LoadChatHistory`
```csharp
public class LoadChatHistoryRequest
{
	public String MessageId { get; set; }
}

public class LoadChatHistoryResponse
{
	public ErrorCode Error { get; set; }
	public List<ChatMessageReceived> ChatHistory { get; set; }
}
```

### 채널 변경
- URI: `/ChannelChange`
```csharp
public class ChannelChangeRequest
{
	public Int32 ChannelNumber { get; set; }
}
public class ChannelChangeResponse
{
	public ErrorCode Error { get; set; }
}
```

## 회고
- [블로그](https://velog.io/@oak_cassia/series/%EC%BB%B4%ED%88%AC%EC%8A%A4-%EC%84%9C%EB%B2%84%EC%BA%A0%ED%8D%BC%EC%8A%A41%EA%B8%B0)
