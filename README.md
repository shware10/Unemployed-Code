# **Auth / Lobby / Network**

* **AuthManager.cs** : UGS 인증 초기화 후 로그인·회원가입을 처리하고 Vivox 초기화·로그인을 함께 수행하는 인증 매니저
* **AuthView.cs** : 로그인/회원가입 UI 제어 및 AuthManager 호출, 성공 시 다음 씬으로 이동하는 UI 컨트롤러
* **LobbyManager.cs** : UGS Lobby·Relay·Vivox·Netcode 통합 관리(방 생성/참가/하트비트/씬 로드)를 담당하는 로비 핵심 매니저
* **LobbyView.cs** : 로비 UI(리스트/검색/생성)를 제어하고 LobbyManager를 통하여 방 생성·참가 등을 수행
* **Room.cs** : 개별 로비 카드 UI, 방 정보 표시와 클릭 시 해당 로비로 참가 처리
* **LobbyKeys.cs** : 로비 데이터 키(JoinCode, State)를 모아둔 상수 클래스
* **StringCleaner.cs** : 문자열의 공백/제로폭/제어 문자를 제거하여 joinCode·ID 입력을 정리
* **PlayerBinder.cs** : 플레이어 스폰 시 GameServer와 UI에 바인드하고 유저 데이터 전송 및 UI 초기화 처리
* **UserData.cs** : 서버에서 관리하는 유저 정보(이름·ID·점수·행동·생존)를 네트워크 직렬화 가능한 구조체로 정의


# **Voice Chat (Vivox)**

* **VivoxManager.cs** : Vivox 초기화·로그인·포지셔널/그룹 채널 조인 및 전환, 참가자 목록 관리 등 음성 시스템 전체 관리
* **VivoxPositionUpdater.cs** : Owner 플레이어의 위치/리스너 방향을 Vivox 서버에 주기적으로 전송하여 포지셔널 오디오 동기화
* **VoiceChatView.cs** : Vivox 참가자 목록 UI 생성 및 말하는 사람의 아이콘/애니메이션 표시

# **Game Server / Gameplay State**

* **GameServer.cs** : 유저 리스트, 게임 상태, 타이머, 맵 로딩, 사망/리스폰 처리 등 멀티플레이 게임 서버 전체 로직 담당


# **Listener Interfaces**

* **IConnectionListener.cs** : 플레이어 접속/퇴장 감지 이벤트를 수신
* **IParticipationListener.cs** : Vivox 참여자 목록 변경 이벤트를 수신
* **IServerStateListener.cs** : 서버 상태 변경 이벤트를 수신
* **ISpeechListener.cs** : 플레이어 음성 감지(말함/말하지 않음) 이벤트를 수신
* **ITimerListener.cs** : 서버 타이머 변경 이벤트를 수신


# **UI / HUD**

* **UIManager.cs** : 서버 상태 변화에 따라 여러 UI(CanvasGroup)를 페이드 인/아웃으로 전환하는 전체 UI 총괄
* **SpectaterView.cs** : 관전 대상 플레이어 이름 표시 및 시스템 메시지 페이드 처리
* **TimerView.cs** : 타이머 변경 시 카운트다운 UI 표시/숨김 처리
* **AlarmView.cs** : 접속/퇴장/게임 상태 변화 알림을 스크롤 UI에 누적 표시
* **MapInfoView.cs** : 세션 시작 시 현재 맵 이름을 일정 시간 UI로 출력
* **MainMenuView.cs** : 메인 메뉴 UI에서 시작/설정/종료 버튼을 제어
* **PlayerUIView.cs** : 배터리·플래시·라디오·스턴 등의 상태 아이콘 표시 및 사망/리스폰 시 UI 반응 처리


# **Check-Out / 결과창**

* **CheckOutManager.cs** : 라운드 종료 시 유저 행동·점수를 기반으로 결과 데이터를 생성하고 ‘특별 메모’(게으른/부지런한 로봇) 부여
* **CheckOutView.cs** : 플레이어 이름/생존 여부/특별 메모 등을 UI로 표시하는 결과 화면 단일 컴포넌트
