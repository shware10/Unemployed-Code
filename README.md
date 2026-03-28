# Unemployed-Code

Unity Netcode + UGS(Authentication, Lobby, Relay, Vivox) 기반 멀티플레이 게임 프로젝트로, 네트워크 동기화, 음성 채팅, 로비 시스템, UI를 구성한 코드 구조입니다.

---

## Folder Structure

### Network

#### Login

UGS 인증 및 로그인 처리

* **AuthManager** : UGS 인증 및 Vivox 초기화/로그인 관리
* **AuthView** : 로그인/회원가입 UI 처리 및 씬 전환

#### Lobby

로비 생성, 참가, Relay 연결 관리

* **LobbyManager** : 로비/Relay/Netcode/Vivox 전체 흐름 관리
* **LobbyView** : 로비 UI 및 방 생성/참가 처리
* **Room** : 개별 방 UI 및 참가 처리
* **ProfileView** : 유저 정보 표시
* **StringCleaner** : 입력 문자열 정리 유틸
* **LobbyKeys** : 로비 데이터 키 정의

#### Vivox

음성 채팅 시스템

* **VivoxManager** : Vivox 초기화 및 채널 관리
* **VivoxPositionUpdater** : 3D 음성 위치 동기화

#### InGame

게임 서버 및 동기화

* **GameServer** : 게임 상태, 타이머, 플레이어 동기화 관리
* **PlayerBinder** : 플레이어와 서버/UI 연결
* **UserData** : 네트워크 동기화용 플레이어 데이터

#### Interface

네트워크 이벤트 전달 인터페이스

* **ITimerListener** : 타이머 변경 이벤트
* **IConnectionListener** : 접속 상태 이벤트
* **IParticipationListener** : 음성 채팅 참가자 이벤트
* **IServerStateListener** : 서버 상태 변경 이벤트
* **ISpeechListener** : 음성 발화 이벤트

---

### UI

게임 전반 UI 시스템

* **UIManager** : 전체 UI 상태 및 전환 관리
* **PlayerUIView** : 플레이어 HUD
* **TimerView** : 타이머 UI
* **MapInfoView** : 맵 정보 표시
* **SpectaterView** : 관전 UI
* **VoiceChatView** : 음성 채팅 UI
* **Chat** : 발화 아이콘 표시
* **AlarmView** : 알림 메시지 UI
* **MainMenuView** : 메인 메뉴 UI
* **CheckOutView** : 정산 결과 UI
* **CheckOutManager** : 정산 결과 로직 관리

---

## 특징

* UGS 기반 멀티플레이 구조 (Authentication / Lobby / Relay / Vivox)
* Netcode for GameObjects 기반 호스트 서버 권한 구조
* 이벤트 기반 UI (Listener 인터페이스 활용)
* 상태 기반 게임 흐름 관리 (ServerState)
* 3D 위치 기반 음성 채팅 시스템
* UI와 로직 분리 (View / Manager 구조)

---
