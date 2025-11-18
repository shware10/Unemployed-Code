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

public enum ServerState
{
    Lobby,
    GameStart,
    SessionStart,
    EscapeStart,
    SessionEnd,
    GameOver,
}

public class GameServer : NetworkBehaviour
{
    public static GameServer Instance;

    /// <summary>
    /// clientId : userdata 맵
    /// </summary>
    public NetworkList<UserData> userList = new();

    /// <summary>
    /// 플레이어 연결 변화 이벤트
    /// </summary>
    public event Action<FixedString64Bytes, bool> OnConnectionChangedEvent;

    /// <summary>
    /// 게임 상태 변화 이벤트
    /// </summary>
    public event Action<ServerState, ServerState> OnStateChangedEvent;

    /// <summary>
    /// 타이머 변화 이벤트
    /// </summary>
    public event Action<int> OnTimerChangedEvent;

    /// <summary>
    ///  맵 변화 이벤트
    /// </summary>
    public event Action<int> OnMapChangedEvent;

    /// <summary>
    /// 게임 상태(자동 동기화)
    /// </summary>
    public NetworkVariable<ServerState> curState = new(ServerState.Lobby);

    public NetworkVariable<int> alivePlayers = new();

    public NetworkVariable<int> time = new(40); // 5분

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

        userList.OnListChanged += OnUserListChanged;
        curState.OnValueChanged += OnGameStateChanged;
        time.OnValueChanged += OnTimerChanged;
        MapIdx.OnValueChanged += OnMapChanged;


        if (IsServer) //연결여부는 각 클라에서 받고 연결 끊기는 서버만 리슨
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnMapChanged(int previousValue, int newValue)
    {
        OnMapChangedEvent?.Invoke(newValue);
    }

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
    /// 호출하면 모두 같이 랜덤맵으로 로드됩니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void NetworkMapLoadServerRpc(bool isSafeZone)
    {
        int idx = isSafeZone ? 0 : Random.Range(1, mapList.Length);
        string nextSceneName = mapList[idx];

        NetworkManager.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);

        MapIdx.Value = idx;
    }


    #endregion

    #region Connection

    /// <summary>
    /// 스폰된 플레이어를 서버에 바인드하는 함수입니다. > 커넥션/게임상태를 리슨합니다.
    /// </summary>
    /// <param name="player"></param>
    public void BindServer(GameObject player)
    {
        IConnectionListener[] Clisteners = player.GetComponentsInChildren<IConnectionListener>(true);
        IServerStateListener[] Slisteners = player.GetComponentsInChildren<IServerStateListener>(true);
        ITimerListener[] TListeners = player.GetComponentsInChildren<ITimerListener>(true);
        
        foreach (var listener in Clisteners) OnConnectionChangedEvent += listener.OnConnctionChanged;
        foreach (var listener in Slisteners) OnStateChangedEvent += listener.OnStateChanged;
        foreach (var listener in TListeners) OnTimerChangedEvent += listener.OnTimerChanged;
    }

    /// <summary>
    /// 유저 접속시 -> 리스트 변화
    /// </summary>
    /// <param name="change"></param>
    private void OnUserListChanged(NetworkListEvent<UserData> change)
    {
        switch (change.Type)
        {
            case NetworkListEvent<UserData>.EventType.Add:
                OnConnectionChangedEvent?.Invoke(change.Value.username, true);
                break;
            case NetworkListEvent<UserData>.EventType.Remove:
                OnConnectionChangedEvent?.Invoke(change.Value.username, false);
                break;
        }
    }

    /// <summary>
    /// 각 클라이언트가 접속 시 유저데이터를 보내는 함수
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="rpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SendDataServerRpc(FixedString64Bytes userName, ServerRpcParams rpcParams = default)
    {
        int senderId = (int)rpcParams.Receive.SenderClientId;
        //리스트에 해당 클라 정보 추가 => OnUserListChanged
        userList.Add(new UserData(userName, rpcParams.Receive.SenderClientId));

        /*
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 4)
        {
            curState.Value = ServerState.GameStart;
        }
        */
    }

    /// <summary>
    /// 클라이언트 접속이 끊길 때 호출되는 함수
    /// </summary>
    /// <param name="clientId"></param>
    private void OnClientDisconnected(ulong clientId)
    {
        //리스트 에서 해당 클라 제거 => OnUserListChanged
        foreach (UserData user in userList)
        {
            if (user.clientId == clientId) userList.Remove(user);
        }

        if (curState.Value != ServerState.Lobby) alivePlayers.Value -= 1;
    }

    #endregion

    #region Server State

    /// <summary>
    /// 게임상태 변경 함수
    /// 호출예시 => 플레이어 안전구역 콜라이더 감지 >> SwithStateServerRpc(GameState.SessionEnd)
    /// </summary>
    /// <param name="newState"></param>
    [ServerRpc(RequireOwnership = false)]
    public void SwitchStateServerRpc(ServerState newState)
    {
        curState.Value = newState;
    }

    /// <summary>
    /// 상태 변경시 호출할 델리게이트 함수
    /// </summary>
    /// <param name="oldState"></param>
    /// <param name="newState"></param>
    private void OnGameStateChanged(ServerState oldState, ServerState newState)
    {
        OnStateChangedEvent?.Invoke(oldState, newState);
        if (IsServer)
        {
            switch (newState)
            {
                case ServerState.SessionStart:
                    alivePlayers.Value = NetworkManager.Singleton.ConnectedClientsList.Count;
                    TimerRoutine = StartCoroutine(TimerStart());
                    break;
                case ServerState.GameOver:
                    // 타이머 종료
                    StartCoroutine(GameOverRoutine());
                    break;
                case ServerState.EscapeStart:
                    // 타이머 종료
                    StopCoroutine(TimerRoutine);
                    break;
                case ServerState.SessionEnd:
                    // 죽은 애들 전부 살리기
                    StartCoroutine(SessionEndRoutine());
                    break;
            }
        }
    }

    IEnumerator GameOverRoutine()
    {
        StopCoroutine(TimerRoutine);
        DieAliveAll();
        yield return new WaitForSeconds(3f);
        TrainManager.Instance?.RequestStartTrainInDungeonServerRpc();
    }

    IEnumerator SessionEndRoutine()
    {
        yield return new WaitForSeconds(3f);
        RespawnInSafeZone();
    }
    

    IEnumerator TimerStart()
    {
        time.Value = 600;
        while (true)
        {
            yield return second;
            time.Value -= 1;
            Debug.Log($"{time.Value}");
        }
    }

    public void OnTimerChanged(int previousValue, int newValue)
    {
        if (newValue <= 30)
        {
            OnTimerChangedEvent?.Invoke(newValue);
        }

        if (newValue == 0)
        {
            StopCoroutine(TimerRoutine);
        
            if(IsServer) curState.Value = ServerState.GameOver;
        }
    }

    

    #endregion

    #region Player

    public void DieAliveAll()
    {
        foreach (UserData user in userList)
        {
            if (user.isAlive == true)
            {
                var player = NetworkManager.Singleton.ConnectedClients[user.clientId].PlayerObject;

                player.gameObject.GetComponent<PlayerPresenterHandler>().SetAcitveCharacter(false);

                DieClientRpc(player.NetworkObjectId);
            }
        }
    }
    
    /// <summary>
    /// 플레이어 hp가 0이 되면 서버에 호출할 함수
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void DieServerRpc(ServerRpcParams rpcParams = default)
    {
        alivePlayers.Value -= 1;

        var player = NetworkManager.Singleton.ConnectedClients[rpcParams.Receive.SenderClientId].PlayerObject;

        player.gameObject.GetComponent<PlayerPresenterHandler>().SetAcitveCharacter(false);

        DieClientRpc(player.NetworkObjectId);

        ulong clientId = rpcParams.Receive.SenderClientId;

        for (int i = 0; i < userList.Count; ++i)
        {
            if (userList[i].clientId == clientId)
            {
                var newUser = userList[i];
                newUser.isAlive = false;
                userList[i] = newUser;
                break;
            }
        }

        if (alivePlayers.Value == 0) curState.Value = ServerState.GameOver;
    }

    [ClientRpc]
    private void DieClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            netObj.gameObject.GetComponent<PlayerPresenterHandler>().SetAcitveCharacter(false);
        }
    }

    private void RespawnInSafeZone()
    {
        for(int i = 0; i < userList.Count; ++i)
        {
            if (!userList[i].isAlive) // 죽어있던 유저 탐색
            {
                var newUser = userList[i];
                newUser.isAlive = true;
                userList[i] = newUser;
                
                var player = NetworkManager.Singleton.ConnectedClients[userList[i].clientId].PlayerObject;
                
                RespawnClientRpc(player.NetworkObjectId, Vector3.zero);
            }
        }
    }


    /// <summary>
    /// 플레이어 머리를 가져가서 리스폰할때 호출할 함수
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RespawnServerRpc(ulong clientId, Vector3 spawnPosition, ServerRpcParams rpcParams = default)
    {
        alivePlayers.Value += 1;

        for(int i = 0; i < userList.Count; ++i)
        {
            if (userList[i].clientId == clientId)
            {
                var newUser = userList[i];
                newUser.isAlive = true;
                userList[i] = newUser;
                break;
            }
        }
        
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        
        RespawnClientRpc(player.NetworkObjectId, spawnPosition);
    }

    [ClientRpc]
    private void RespawnClientRpc(ulong networkObjectId, Vector3 spawnPosition)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out var netObj))
        {
            PlayerPresenterHandler playerPresenterHandler = netObj.gameObject.GetComponent<PlayerPresenterHandler>();
            netObj.gameObject.transform.position = Vector3.zero;
            playerPresenterHandler.SetAcitveCharacter(true);
            playerPresenterHandler.SetisRespawnState(true);
            
            Debug.Log("리스폰됨");
        }
    }
    
    /// <summary>
    /// 정산 시 표시될 스코어와 액션 수
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void OnScoredServerRpc(int score, int action, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        for(int i = 0; i < userList.Count; ++i)
        {
            if (userList[i].clientId == clientId)
            {
                var newUser = userList[i];
                newUser.action = action;
                newUser.score = score;
                userList[i] = newUser;
                break;
            }
        }
    }

    #endregion
}