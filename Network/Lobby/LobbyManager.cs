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

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;
    public Lobby CurrentLobby;
    private Coroutine heartbeatCo;
	
	private string safeSceneName = "IngameScene_Subway-1(Safe)";

    void Awake()
    {
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
    /// 방 생성 함수
    /// </summary>
    /// <param name="lobbyName">로비 이름</param>
    /// <param name="maxPlayers">최대 인원</param>
    /// <param name="isPrivate">비공개/공개 여부</param>
    /// <returns></returns>
    public async Task<bool> CreateRoomAsync(string lobbyName, int maxPlayers, bool isPrivate = false)
    {
        if (NetworkManager.Singleton.IsListening) NetworkManager.Singleton.Shutdown();
        try
        {
            //릴레이 세션 할당
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

            //조인 코드 가져와서
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            
            // 로비 생성
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(
                lobbyName, maxPlayers,
                new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Data = new Dictionary<string, DataObject>
                    {
                        { LobbyKeys.State , new DataObject(DataObject.VisibilityOptions.Public, "Lobby") }, //상태가 '로비'인 로비만 탐색
                        { LobbyKeys.JoinCode, new DataObject(DataObject.VisibilityOptions.Member, joinCode) }
                    }
                });

            //하트비트 시작            
            heartbeatCo = StartCoroutine(Heartbeat(CurrentLobby.Id));

            //utp 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(alloc, "dtls"));
            //씬 로드
            await LoadSceneAndWaitAsync(safeSceneName);
            
            // 호스트 시작 
            NetworkManager.Singleton.StartHost();
            
            await VivoxManager.Instance.VivoxJoinPositionalChannelAsync(joinCode);
            await Task.Delay(1000);
            await VivoxManager.Instance.VivoxJoinGroupChannelAsync(joinCode);
            
            await VivoxService.Instance.SetChannelTransmissionModeAsync(
                TransmissionMode.Single, VivoxManager.Instance.positionalChannelName);
            
            //조인코드 출력
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
    ///  로비 유지 여부를 확인하는 하트비트
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <returns></returns>
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
    /// 로비 상태를 인게임으로 스위치하는 함수
    /// </summary>
    public async Task SetInGameAsync()
    {
        if (CurrentLobby == null) return;
        await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { LobbyKeys.State, new DataObject(DataObject.VisibilityOptions.Public, "InGame") } // 상태를 '인게임'으로 변경
            }
        });
    }
    
    /// <summary>
    /// 로비 삭제 함수
    /// </summary>
    public async Task CloseAsync()
    {
        //하트비트 종료
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
    
    //게임 강제 종료 시 로비 삭제
    private async void OnApplicationQuit() => await CloseAsync();
    
    #endregion
    
    #region Client
    /// <summary>
    /// 로비 불러오기 로직
    /// </summary>
    /// <returns></returns>
    public async Task<List<Lobby>> RefreshLobbiesAsync()
    {
        try
        {
            var res = await LobbyService.Instance.QueryLobbiesAsync(new QueryLobbiesOptions
            {
                Count = 25, //불러올 로비 갯수
                Filters = new List<QueryFilter>()
                {
                    // new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT), //greater than 0 => 남은 자리가 0 이상인 로비 
                    // new QueryFilter(QueryFilter.FieldOptions.S1, "Lobby", QueryFilter.OpOptions.EQ), // equal lobby => S1 값이 로비인 방  
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created) //최신순으로 내림차순
                }
            });
            return res.Results ?? new List<Lobby>(); // res.Result == null 이면 빈 로비 리스트 반환
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로비 쿼리 실패 {e}");
            return new List<Lobby>();
        }
    }
    
    /// <summary>
    /// 로비 코드 참가 로직
    /// </summary>
    /// <param name="joinCode"></param>
    public async Task<bool> JoinByCodeAsync(string joinCode)
    {
        string code = StringCleaner.Clean(joinCode);
        
        Debug.Log(joinCode);
        if (string.IsNullOrEmpty(joinCode)) return false;
        
        try
        {
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAlloc, "dtls"));

            //씬 로드
            await LoadSceneAndWaitAsync(safeSceneName);
            
            //클라이언트 시작
            NetworkManager.Singleton.StartClient();
            
            // Vivox 채널 조인
            await VivoxManager.Instance.VivoxJoinPositionalChannelAsync(joinCode);
            await Task.Delay(1000);
            await VivoxManager.Instance.VivoxJoinGroupChannelAsync(joinCode);
            
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
    /// 로비 클릭 참가 로직
    /// </summary>
    /// <param name="lobby"></param>
    /// <returns></returns>
    public async Task<bool> JoinByClickAsync(Lobby lobby)
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);

            if (joinedLobby.Data == null || !joinedLobby.Data.TryGetValue(LobbyKeys.JoinCode, out DataObject data) ||
                string.IsNullOrEmpty(data.Value))
            {
                Debug.Log("해당 로비는 존재하지 않습니다");
                return false;
            }

            string joinCode = StringCleaner.Clean(data.Value);
            
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(data.Value);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAlloc, "dtls"));

            //씬 로드
            await LoadSceneAndWaitAsync(safeSceneName);
            
            //호스트 시작
            NetworkManager.Singleton.StartClient();
            
            // Vivox 채널 조인
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
    /// 씬 로드가 완전히 완료될 떼까지 기다리는 함수
    /// </summary>
    private async Task LoadSceneAndWaitAsync(string sceneName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            await Task.Yield();
        await Task.Delay(100); // 한 프레임 여유
    }
    #endregion
}