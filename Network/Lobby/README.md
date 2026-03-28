# Lobby

로비 생성, 참가, 조회 및 네트워크 연결(Relay/Vivox)을 관리하는 스크립트 모음입니다.

## Scripts

* **LobbyManager**
  로비 생성/참가, Relay 연결, Netcode 시작, Vivox 채널 연결까지 전체 네트워크 흐름을 관리하는 핵심 매니저 

* **LobbyView**
  로비 리스트 UI 생성, 방 생성/참가/검색 및 패널 전환을 처리하는 UI 컨트롤러 

* **Room**
  개별 로비(방) 정보를 UI에 표시하고 클릭 시 참가를 처리하는 클래스 

* **ProfileView**
  현재 로그인된 유저 이름을 UI에 표시하는 클래스 

* **StringCleaner**
  입력 문자열에서 공백 및 불필요한 문자를 제거하는 유틸 클래스 

* **LobbyKeys**
  로비 데이터에서 사용하는 키(JoinCode, State)를 정의한 상수 클래스 
