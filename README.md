# DungeonWarAPI-정휘찬
![Dungeon War API](https://github.com/gnlckswjd/Project-DungeonWarAPI/assets/52772732/1d72f22c-7a11-471b-960b-26c3e23f3db6)



## 프로젝트 개요
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

## 주요 기능
- 모든 요청은 POST 요청입니다.

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

