using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine.SceneManagement;

/// <summary>
/// 로비 / 릴레이 / 네트워크 연결 관리
/// </summary>
public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    /// <summary> 현재 참여중인 로비 </summary>
    public Lobby CurrentLobby;

    // 로비 유지용 하트비트 코루틴
    private Coroutine heartbeatCo;
	
	// 게임 시작 시 전환될 씬
    private string safeSceneName = "IngameScene_Subway-1(Safe)";

    void Awake()
    {
        // 싱글톤 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    #region Host

    /// <summary>
    /// 방 생성
    /// </summary>
    public async Task<bool> CreateRoomAsync(string lobbyName, int maxPlayers, bool isPrivate = false)
    {
        // 기존 네트워크 종료
        if (NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        try
        {
            // Relay 할당
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            // 클라이언트가 사용할 조인 코드 생성
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            
            // 로비 생성 + JoinCode 저장
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName, maxPlayers,
                new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Data = new Dictionary<string, DataObject>
                    {
                        { LobbyKeys.State , new DataObject(DataObject.VisibilityOptions.Public, "Lobby") },
                        { LobbyKeys.JoinCode, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
                });

            // 로비 유지
            heartbeatCo = StartCoroutine(Heartbeat(CurrentLobby.Id));

            // Netcode Relay 연결 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(alloc, "dtls"));

            // 씬 로드 후
            await LoadSceneAndWaitAsync(safeSceneName);
            
            // 호스트 시작
            NetworkManager.Singleton.StartHost();

            // Vivox 채널 참여 (거리 + 무전기)
            await VivoxManager.Instance.VivoxJoinPositionalChannelAsync(joinCode);
            await Task.Delay(1000);
            await VivoxManager.Instance.VivoxJoinGroupChannelAsync(joinCode);

            // 거리 기반 채널을 기본 송신 채널 설정
            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.Single, VivoxManager.Instance.positionalChannelName);

            Debug.Log($"JoinCode : {joinCode}");
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[LobbyHost] CreateRoom failed : {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 로비 유지용 하트비트 (15초마다)
    /// </summary>
    private IEnumerator Heartbeat(string lobbyId)
    {
        WaitForSecondsRealtime wait = new WaitForSecondsRealtime(15f);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return wait;
        }
    }

    /// <summary>
    /// 로비 상태를 인게임으로 변경
    /// </summary>
    public async Task SetInGameAsync()
    {
        if (CurrentLobby == null) return;

        await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { LobbyKeys.State, new DataObject(DataObject.VisibilityOptions.Public, "InGame") }
            }
        });
    }
    
    /// <summary>
    /// 로비 삭제 + 정리
    /// </summary>
    public async Task CloseAsync()
    {
        // 하트비트 종료
        if (heartbeatCo != null)
        {
            StopCoroutine(heartbeatCo);
            heartbeatCo = null;
        }
        
        // 로비 삭제
        if (CurrentLobby != null)
        {
            await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
        }

        CurrentLobby = null;
    }
    
    // 게임 종료 시 로비 자동 삭제
    private async void OnApplicationQuit() => await CloseAsync();
    
    #endregion
    
    #region Client

    /// <summary>
    /// 로비 목록 조회
    /// </summary>
    public async Task<List<Lobby>> RefreshLobbiesAsync()
    {
        try
        {
            var res = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
            {
                Count = 25,
                Order = new List<QueryOrder>
                {
                    // 최신순 정렬
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            });

            // null 방지
            return res.Results ?? new List<Lobby>();
        }
        catch (Exception e)
        {
            Debug.LogError($"로비 쿼리 실패 {e}");
            return new List<Lobby>();
        }
    }
    
    /// <summary>
    /// 코드로 로비 참가
    /// </summary>
    public async Task<bool> JoinByCodeAsync(string joinCode)
    {
        string code = StringCleaner.Clean(joinCode);

        if (string.IsNullOrEmpty(code)) return false;
        
        try
        {
            // Relay 참가
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);

            // Netcode Relay 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(joinAlloc, "dtls"));

            // 씬 로드
            await LoadSceneAndWaitAsync(safeSceneName);
            
            // 클라이언트 시작
            NetworkManager.Singleton.StartClient();
            
            // Vivox 채널 참여
            await VivoxManager.Instance.VivoxJoinPositionalChannelAsync(code);
            await Task.Delay(1000);
            await VivoxManager.Instance.VivoxJoinGroupChannelAsync(code);

            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.Single, VivoxManager.Instance.positionalChannelName);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"코드로 조인하기 실패: {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 로비 클릭으로 참가
    /// </summary>
    public async Task<bool> JoinByClickAsync(Lobby lobby)
    {
        try
        {
            // 로비 참여
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            // JoinCode 추출
            if (joinedLobby.Data == null ||
                !joinedLobby.Data.TryGetValue(LobbyKeys.JoinCode, out DataObject data) ||
                string.IsNullOrEmpty(data.Value))
            {
                Debug.Log("해당 로비는 존재하지 않습니다");
                return false;
            }

            string joinCode = StringCleaner.Clean(data.Value);

            // Relay 참가
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Netcode 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(AllocationUtils.ToRelayServerData(joinAlloc, "dtls"));

            // 씬 로드
            await LoadSceneAndWaitAsync(safeSceneName);
            
            // 클라이언트 시작
            NetworkManager.Singleton.StartClient();
            
            // Vivox 참여
            await VivoxManager.Instance.VivoxJoinPositionalChannelAsync(joinCode);
            await Task.Delay(1000);
            await VivoxManager.Instance.VivoxJoinGroupChannelAsync(joinCode);

            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.Single, VivoxManager.Instance.positionalChannelName);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"클릭으로 조인하기 실패 : {e}");
            return false;
        }
    }

    /// <summary>
    /// 씬 로드 완료까지 대기
    /// </summary>
    private async Task LoadSceneAndWaitAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로딩 완료까지 대기
        while (!op.isDone)
            await Task.Yield();

        // 안정성 확보용 한 프레임 대기
        await Task.Delay(100);
    }

    #endregion
}