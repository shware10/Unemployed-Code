using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Services.Authentication;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

/// <summary> 서버 상태 열거형 </summary>
public enum ServerState
{
    Lobby,
    GameStart,
    SessionStart,
    EscapeStart,
    SessionEnd,
    GameOver,
}

/// <summary>
/// Netcode 네트워크 API를 바탕으로 동기화를 관리하는 싱글턴 클래스
/// </summary>
public class GameServer : NetworkBehaviour
{
    public static GameServer Instance;

    /// <summary> clientId : userdata 맵 </summary>
    public NetworkList<UserData> userList = new();

    /// <summary> 플레이어 연결 변화 이벤트 </summary>
    public event Action<FixedString64Bytes, bool> OnConnectionChangedEvent;

    /// <summary> 게임 상태 변화 이벤트 </summary>
    public event Action<ServerState, ServerState> OnStateChangedEvent;

    /// <summary> 타이머 변화 이벤트 </summary>
    public event Action<int> OnTimerChangedEvent;

    /// <summary> 맵 변화 이벤트 </summary>
    public event Action<int> OnMapChangedEvent;

    /// <summary> 게임 상태 자동 동기화 </summary>
    public NetworkVariable<ServerState> curState = new(ServerState.Lobby);

    /// <summary> 현재 생존 플레이어 수 </summary>
    public NetworkVariable<int> alivePlayers = new();

    public NetworkVariable<int> time = new(40);

    /// <summary> 현재 맵 인덱스 </summary>
    public NetworkVariable<int> MapIdx = new(0);

    private WaitForSeconds second;
    private Coroutine TimerRoutine;
    private Scene curScene;
    private string[] mapList = new string[4]
    {
        "IngameScene_Subway-1(Safe)",
        "IngameScene_Subway-2(Farming)",
        "IngameScene_Subway-3(Farming)",
        "IngameScene_Subway-4(Farming)"
    };

    void Awake()
    {
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        second = new WaitForSeconds(1);
    }

    public override void OnNetworkSpawn()
    {
        // NetworkVariables 계열은 여기서 등록
        // 값은 서버에서 바꾸고 클라에서는 리슨

        userList.OnListChanged  += OnUserListChanged;
        curState.OnValueChanged += OnGameStateChanged;
        time.OnValueChanged     += OnTimerChanged;
        MapIdx.OnValueChanged   += OnMapChanged;


        if (IsServer) //연결여부는 각 클라에서 받고 연결 끊기는 서버만 리슨
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    // 맵 변경 이벤트 발생 시 처리될 함수
    private void OnMapChanged(int previousValue, int newValue)
    {
        OnMapChangedEvent?.Invoke(newValue);
    }

    /// <summary>
    /// clientId > username 조회
    /// </summary>
    public string GetUserName(ulong clientId)
    {
        foreach (UserData userData in userList)
        {
            if (userData.clientId == clientId) return userData.username.ToString();
        }

        return null;
    }

#region Map

    /// <summary>
    /// 서버에서 맵 로드 > 모든 클라 동기화
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void NetworkMapLoadServerRpc(bool isSafeZone)
    {
        // 안전지대면 0번, 아니면 랜덤 맵 선택
        int idx = isSafeZone ? 0 : Random.Range(1, mapList.Length);

        // 모든 클라이언트 씬 동기화 로드
        NetworkManager.SceneManager.LoadScene(mapList[idx], LoadSceneMode.Single);

        // 현재 맵 인덱스 동기화
        MapIdx.Value = idx;
    }

#endregion


#region Connection

    /// <summary>
    /// 플레이어 객체를 서버 이벤트에 연결
    /// (UI, 상태, 타이머 리스너 등록)
    /// </summary>
    public void BindServer(GameObject player)
    {
        // 플레이어 내부 리스너 찾아서 서버 이벤트에 등록
        foreach (var l in player.GetComponentsInChildren<IConnectionListener>(true))
            OnConnectionChangedEvent += l.OnConnctionChanged;

        foreach (var l in player.GetComponentsInChildren<IServerStateListener>(true))
            OnStateChangedEvent += l.OnStateChanged;

        foreach (var l in player.GetComponentsInChildren<ITimerListener>(true))
            OnTimerChangedEvent += l.OnTimerChanged;
    }


    // 유저 리스트 변화 이벤트 발생 시 처리될 함수
    private void OnUserListChanged(NetworkListEvent<UserData> change)
    {
        // 유저 추가/삭제 시 UI 등에 이벤트 전달
        if (change.Type == NetworkListEvent<UserData>.EventType.Add)
            OnConnectionChangedEvent?.Invoke(change.Value.username, true);

        if (change.Type == NetworkListEvent<UserData>.EventType.Remove)
            OnConnectionChangedEvent?.Invoke(change.Value.username, false);
    }

    /// <summary>
    /// 클라이언트 > 서버 유저 데이터 전달
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SendDataServerRpc(FixedString64Bytes userName, ServerRpcParams rpcParams = default)
    {
        // 클라이언트가 보낸 username으로 유저 데이터 생성 후 리스트에 추가
        userList.Add(new UserData(userName, rpcParams.Receive.SenderClientId));
    }

    // 클라이언트 연결 해제 이벤트 발생 시 처리될 함수
    private void OnClientDisconnected(ulong clientId)
    {
        // 연결 끊긴 유저 리스트에서 제거
        foreach (UserData user in userList)
        {
            if (user.clientId == clientId)
                userList.Remove(user);
        }

        // 게임 중이면 생존자 수 감소
        if (curState.Value != ServerState.Lobby)
            alivePlayers.Value -= 1;
    }

#endregion


#region Server State

    /// <summary>
    /// 상태 변경 (클라 → 서버 요청)
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void SwitchStateServerRpc(ServerState newState)
    {
        // 서버 상태 변경
        curState.Value = newState;
    }
    
    // 게임 상태 변경 이벤트 발생 시 처리될 함수
    private void OnGameStateChanged(ServerState oldState, ServerState newState)
    {
        // 모든 리스너에게 상태 변경 알림
        OnStateChangedEvent?.Invoke(oldState, newState);

        if (!IsServer) return;

        switch (newState)
        {
            case ServerState.SessionStart:
                // 현재 접속 인원 = 생존자 수
                alivePlayers.Value = NetworkManager.Singleton.ConnectedClientsList.Count;

                // 타이머 시작
                TimerRoutine = StartCoroutine(TimerStart());
                break;

            case ServerState.GameOver:
                // 게임 종료 처리
                StartCoroutine(GameOverRoutine());
                break;

            case ServerState.EscapeStart:
                // 타이머 중지
                StopCoroutine(TimerRoutine);
                break;

            case ServerState.SessionEnd:
                // 세션 종료 > 리스폰 처리
                StartCoroutine(SessionEndRoutine());
                break;
        }
    }
    
    // 게임 오버 상태 전환시 실행될 루틴
    IEnumerator GameOverRoutine() 
    {
        StopCoroutine(TimerRoutine);
        DieAliveAll();
        yield return new WaitForSeconds(3f);
        // 기차 애니메이션 실행
        TrainManager.Instance?.RequestStartTrainInDungeonServerRpc();
    }
    
    IEnumerator SessionEndRoutine()
    {
        yield return new WaitForSeconds(3f); 
        RespawnInSafeZone();
    }

    IEnumerator TimerStart()
    {
        // 초기 시간 설정
        time.Value = 600;

        while (true)
        {
            yield return second;

            // 매초 시간 감소
            time.Value -= 1;
        }
    }
    
    /// <summary>
    /// 타이머 변화 이벤트 발생 시 처리될 함수
    /// </summary>
    public void OnTimerChanged(int _, int newValue)
    {
        // 30초 이하부터 UI 업데이트
        if (newValue <= 30)
            OnTimerChangedEvent?.Invoke(newValue);

        if (newValue == 0)
        {
            // 타이머 종료 > 게임 종료 상태 전환
            StopCoroutine(TimerRoutine);
            if (IsServer) curState.Value = ServerState.GameOver;
        }
    }

    #endregion


    #region Player
    
    /// <summary>
    /// 모든 생존 플레이어 사망 처리
    /// </summary>
    public void DieAliveAll()
    {
        foreach (UserData user in userList)
        {
            if (!user.isAlive) continue;

            var player = NetworkManager.Singleton.ConnectedClients[user.clientId].PlayerObject;

            // 캐릭터 비활성화
            player.GetComponent<PlayerPresenterHandler>().SetAcitveCharacter(false);

            // 모든 클라에 사망 상태 전달
            DieClientRpc(player.NetworkObjectId);
        }
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void DieServerRpc(ServerRpcParams rpcParams = default)
    {
        // 생존자 수 감소
        alivePlayers.Value -= 1;

        ulong clientId = rpcParams.Receive.SenderClientId;
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        // 서버에서 캐릭터 비활성화
        player.GetComponent<PlayerPresenterHandler>().SetAcitveCharacter(false);

        // 클라이언트에도 반영
        DieClientRpc(player.NetworkObjectId);

        // userList 상태 업데이트 (isAlive = false)
        for (int i = 0; i < userList.Count; i++)
        {
            if (userList[i].clientId == clientId)
            {
                var u = userList[i];
                u.isAlive = false;
                userList[i] = u;
                break;
            }
        }

        // 전원 사망 시 게임 종료
        if (alivePlayers.Value == 0)
            curState.Value = ServerState.GameOver;
    }
    
    /// <summary>
    /// 플레이어 사망 시 비활성화 동기화
    /// </summary>
    /// <param name="networkObjectId"></param>
    /// <returns></returns>
    [ClientRpc]
    private void DieClientRpc(ulong networkObjectId)
    {
        // 해당 플레이어 찾아서 비활성화
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            netObj.GetComponent<PlayerPresenterHandler>().SetAcitveCharacter(false);
        }
    }
    /// <summary>
    /// 사망 유저 리스폰
    /// </summary>
    private void RespawnInSafeZone()
    {
        for (int i = 0; i < userList.Count; i++)
        {
            if (userList[i].isAlive) continue;

            // 사망 > 생존 상태로 복구
            var u = userList[i];
            u.isAlive = true;
            userList[i] = u;

            var player = NetworkManager.Singleton.ConnectedClients[u.clientId].PlayerObject;

            // 클라이언트 리스폰 처리
            RespawnClientRpc(player.NetworkObjectId, Vector3.zero);
        }
    }

    [ClientRpc]
    private void RespawnClientRpc(ulong id, Vector3 pos)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var netObj))
        {
            var handler = netObj.GetComponent<PlayerPresenterHandler>();

            // 위치 초기화 + 활성화
            netObj.transform.position = pos;
            handler.SetAcitveCharacter(true);
            handler.SetisRespawnState(true);
        }
    }

    /// <summary>
    /// 점수/행동량 업데이트 함수
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void OnScoredServerRpc(int score, int action, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        // 점수 / 행동량 업데이트
        for (int i = 0; i < userList.Count; i++)
        {
            if (userList[i].clientId == clientId)
            {
                var u = userList[i];
                u.score = score;
                u.action = action;
                userList[i] = u;
                break;
            }
        }
    }

#endregion
}