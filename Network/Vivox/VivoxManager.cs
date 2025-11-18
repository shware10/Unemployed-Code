using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance;

    // 가청거리 -> 어디까지 목소리가 들릴지 (0 < this)
    [SerializeField] private int audibleDistance = 32;

    // 대화적거리 어디까지 보통 목소리가 들릴지 == 어디서부터 희미해질지 (0 <= this <= audible)
    [SerializeField] private int conversationalDistance = 1;

    // 값이 1.0보다 크면 대화 거리에서 멀어질수록 오디오가 더 빨리 사라지고, 값이 1.0보다 작으면 오디오가 더 느리게 사라집니다. 기본값은 1.0입니다.
    [SerializeField] private float audioFadeIntensityByDistanceaudio = 1.0f;

    //캐싱할 채널 이름
    public string positionalChannelName;   // 디폴트
    private string groupChannelName;        // 무전기용
    
    private bool vivoxInitialized = false;
    private bool isGroupJoined;
    private bool isPositionalJoined;

    public Action<List<VivoxParticipant>> OnParticipantChangedEvent;
    public Action<string, bool> OnSpeechDetectedEvent;
    
    public List<VivoxParticipant> participantsList; 
    
    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        participantsList = new List<VivoxParticipant>();
    }

    private void Start()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        participantsList.Add(participant);
        OnParticipantChangedEvent?.Invoke(participantsList);
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        participantsList.Remove(participant);
        OnParticipantChangedEvent?.Invoke(participantsList);
    }
    
    public async Task SwitchChannelAsync(bool isWalkieTalkie)
    {
        if (isWalkieTalkie)
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, groupChannelName);
            Debug.Log("vivox 무전기로 전환");
        }
        else
        {
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, positionalChannelName);
            Debug.Log("vivox 포지셔널 전환");
        }
    }

    /// <summary>
    /// Vivox 서버 초기화 함수 > UGS 초기화 이후에 호출되어야함
    /// </summary>
    public async Task InitializeAsync()
    {
        await VivoxService.Instance.InitializeAsync();
        vivoxInitialized = true;
    }

    /// <summary>
    /// Vivox 로그인 함수 > Vivox 초기화 이후에 호출되어야함
    /// </summary>
    public async Task VivoxLoginAsync()
    {
        if (!vivoxInitialized || VivoxService.Instance.IsLoggedIn) return;

        try
        {
            var options = new LoginOptions()
            {
                DisplayName = AuthManager.Instance.userName
            };
            await VivoxService.Instance.LoginAsync(options);
            Debug.Log("Vivox 로그인");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 로그인 실패 : {e}");
        }
    }
    
    /// <summary>
    /// Vivox 로그아웃 함수
    /// </summary>
    public async Task VivoxLogoutAsync()
    {
        if (!VivoxService.Instance.IsLoggedIn) return;
        try
        {
            await VivoxService.Instance.LogoutAsync();
            Debug.Log("Vivox 로그아웃");
        }
        catch (Exception e)
        {
            Debug.LogError("Vivox 로그아웃 실패");
        }
    }

    /// <summary>
    /// <br> Vivox 채널 Join/Create 함수 Vivox는 별도로 채널을 만들지 않아도 조인시 해당 채널명이 없으면 새로 만듭니다. </br>
    /// <br> 그룹 채널로 거리에 상관 없는 무전기 채널로 사용될 예정입니다. </br>
    /// </summary>
    /// <param name="joinCode"> 채널 이름은 서버의 조인코드 기반입니다. </param>
    public async Task VivoxJoinGroupChannelAsync(string joinCode)
    {
        if (!vivoxInitialized || !VivoxService.Instance.IsLoggedIn) return;

        groupChannelName = joinCode + "_group";

        try
        {
            await VivoxService.Instance.JoinGroupChannelAsync(groupChannelName, ChatCapability.AudioOnly);
            isGroupJoined = true;
            Debug.Log($"Vivox 그룹 채널 참여 완료: {groupChannelName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 그룹 채널 참여 실패 {e}");
        }
    }

    public async Task VivoxJoinPositionalChannelAsync(string joinCode)
    {
        if (!vivoxInitialized || !VivoxService.Instance.IsLoggedIn) return;
        
        positionalChannelName = joinCode + "_positional";
        isPositionalJoined = true;
        try
        {
            await VivoxService.Instance.JoinPositionalChannelAsync(
                positionalChannelName,
                ChatCapability.AudioOnly,
                new Channel3DProperties
                (
                    audibleDistance,
                    conversationalDistance,
                    audioFadeIntensityByDistanceaudio,
                    AudioFadeModel.InverseByDistance)
                );
            Debug.Log($"Vivox 포지셔널 채널 참여 완료: {positionalChannelName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 포지셔널 채널 참여 실패 {e}");
        }
        
    }
    
    
    /// <summary>
    /// 모든 채널 떠나기 함수 > 로비를 떠날 때 호출되어야함
    /// </summary>
    public async Task VivoxLeaveAllChannelsAsync() {
        if (isGroupJoined) await VivoxService.Instance.LeaveChannelAsync(groupChannelName);
        if (isPositionalJoined) await VivoxService.Instance.LeaveChannelAsync(positionalChannelName);
        isGroupJoined = isPositionalJoined = false;
    }
    
    /// <summary>
    /// Vivox 채널 leave 함수 
    /// </summary>
    /// <param name="channelName">채널 이름은 서버의 조인코드입니다.</param>
    public async Task VivoxLeaveChannelAsync(string channelName)
    {
        try
        {
            await VivoxService.Instance.LeaveChannelAsync(channelName);
            Debug.Log($"Vivox 채널 떠남: {channelName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 채널 떠나기 실패 {e}");
        }
    }
}
