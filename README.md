1. Authentication & Lobby
AuthManager

UGS 인증을 초기화하고 로그인/회원가입을 처리하며 Vivox도 함께 초기화·로그인하는 인증 매니저.

AuthView

로그인·회원가입 UI에서 AuthManager를 호출해 인증을 진행하고 성공 시 다음 씬으로 이동하는 UI 컨트롤러.

LobbyManager

UGS Lobby/Relay/Vivox/Netcode를 통합 관리하며 방 생성·참가·하트비트·씬 로드를 전담하는 로비 핵심 매니저.

LobbyView

로비 UI 전체(리스트/생성/검색)를 제어하고 LobbyManager를 호출해 방 생성·참가·새로고침을 수행.

Room

로비 리스트의 단일 방 카드 UI로, 방 정보 표시 및 클릭 시 해당 로비에 참가하도록 처리.

LobbyKeys

로비 데이터 키(JoinCode, State)를 모아둔 상수 클래스.

StringCleaner

문자열에서 공백·제어·제로폭 문자를 제거해 joinCode나 ID 입력을 안전하게 정리.

2. Voice Chat (Vivox)
VivoxManager

Vivox 초기화·로그인·포지셔널/그룹 채널 조인·전환·참가자 목록 관리 등을 담당하는 음성채팅 총괄 매니저.

VivoxPositionUpdater

Owner 플레이어의 위치·리스너 정보를 Vivox로 주기적으로 보내 포지셔널 오디오를 동기화.

VoiceChatView

Vivox 채널 참가자 목록 UI를 구성하고, 말하는 사람에게 애니메이션/아이콘을 표시하는 음성 UI.

3. Network / Game Server Logic
GameServer

네트워크 게임의 핵심 서버 로직으로,
유저 리스트, 게임 상태, 타이머, 맵 로딩, 사망/리스폰, 세션 시작·종료 처리 등을 통합 관리하는 서버 매니저.

PlayerBinder

플레이어가 스폰될 때 서버에 Listener들을 바인드하고 유저데이터 전송, UI 초기화를 수행하는 바인딩 스크립트.

UserData

서버가 관리하는 유저 정보(이름·ID·점수·액션·생존)를 네트워크 직렬화 가능한 구조체로 정의.

4. Listener Interfaces
IConnectionListener

플레이어 접속/퇴장 이벤트를 수신하는 리스너.

IParticipationListener

Vivox 음성 채널 참가자 목록 변화 이벤트를 수신.

IServerStateListener

게임 상태 변화 이벤트를 수신.

ISpeechListener

플레이어 음성 감지(말함/안 말함) 이벤트를 수신.

ITimerListener

서버 타이머 변경 이벤트를 수신.

5. Player UI / HUD
PlayerUIView

배터리·플래시·라디오·네트·스턴 UI를 표시하며, 사망/리스폰 시 UIManager와 연동하여 상태 UI를 갱신.

SpectaterView

관전 대상의 이름을 UI에 표시하고 시스템 다운 메시지를 페이드 아웃하는 관전 UI.

TimerView

서버 타이머 변화에 따라 카운트다운 UI를 표시/숨김 처리하는 타이머 뷰.

6. Game UI / System UI
UIManager

게임 상태 변화에 따라 여러 CanvasGroup(UI 패널)을 페이드 인/아웃으로 자연스럽게 전환하는 전체 UI 총괄.

AlarmView

플레이어 접속/퇴장/게임 상태 변경 로그를 스크롤 UI에 순차적으로 출력하는 알람 시스템.

MapInfoView

세션 시작 시 현재 맵 정보를 몇 초간 UI로 표시.

MainMenuView

메인 메뉴에서 시작/설정/종료 버튼을 처리하며 적절한 패널을 전환.

7. Check-Out / 결과화면
CheckOutManager

라운드 종료 시 유저들의 행동 수/점수를 분석해 ‘가장 게으른 로봇’, ‘가장 부지런한 로봇’ 같은 메모를 부여하고 결과 데이터 생성.

CheckOutView

플레이어 이름·생존 여부·특별 메모를 UI로 표시하는 결과 화면의 단일 뷰 컴포넌트.
