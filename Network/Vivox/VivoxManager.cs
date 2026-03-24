using System; 
using System.Collections.Generic; 
using System.Threading.Tasks; 
using Unity.Collections; 
using Unity.Services.Vivox; 
using UnityEngine;

/// <summary>
/// Vivox 음성 채팅 관리 싱글톤 클래스
/// </summary>
public class VivoxManager : MonoBehaviour
{
    public static VivoxManager Instance;

    // 최대 가청 거리
    [SerializeField] private int audibleDistance = 32;

    // 일반 대화 거리 (이 거리부터 볼륨 감소 시작)
    [SerializeField] private int conversationalDistance = 1;

    // 거리별 볼륨 감소 강도 (1 = 기본)
    [SerializeField] private float audioFadeIntensityByDistanceaudio = 1.0f;

    /// <summary> 거리 기반 채널 (기본 음성 채팅) </summary>
    public string positionalChannelName;

    // 그룹 채널 (무전기) 
    private string groupChannelName;
    
    // 초기화 판단 변수
    private bool vivoxInitialized = false;
    // 그룹 채널 조인 판단 변수
    private bool isGroupJoined;
    // 거리 채널 조인 판단 변수
    private bool isPositionalJoined;

    /// <summary> 참가자 리스트 변경 이벤트 </summary>
    public Action<List<VivoxParticipant>> OnParticipantChangedEvent;

    /// <summary> 말하기 감지 이벤트 </summary>
    public Action<string, bool> OnSpeechDetectedEvent;

    /// <summary> 현재 채널 참가자 목록 </summary>
    public List<VivoxParticipant> participantsList;

    private async void Awake()
    {
        // 싱글톤 유지
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
        // 참가자 입장/퇴장 이벤트 등록
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }
    
    // 참가자 입장 이벤트 발생 시 실행될 메서드
    private void OnParticipantAdded(VivoxParticipant participant)
    {
        // 참가자 추가 후 UI 갱신
        participantsList.Add(participant);
        OnParticipantChangedEvent?.Invoke(participantsList);
    }
    
    // 참가자 퇴장 이벤트 발생 시 실행될 메서드
    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        // 참가자 제거 후 UI 갱신
        participantsList.Remove(participant);
        OnParticipantChangedEvent?.Invoke(participantsList);
    }

    /// <summary>
    /// 채널 송신 대상 변경 (무전기 <> 거리채팅)
    /// </summary>
    public async Task SwitchChannelAsync(bool isWalkieTalkie)
    {
        if (isWalkieTalkie)
        {
            // 무전기 채널로 송신
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, groupChannelName);
        }
        else
        {
            // 거리 기반 채널로 송신
            await VivoxService.Instance.SetChannelTransmissionModeAsync(TransmissionMode.Single, positionalChannelName);
        }
    }

    /// <summary>
    /// Vivox 초기화 *UGS 초기화 이후 호출*
    /// </summary>
    public async Task InitializeAsync()
    {
        await VivoxService.Instance.InitializeAsync();
        vivoxInitialized = true;
    }

    /// <summary>
    /// Vivox 로그인
    /// </summary>
    public async Task VivoxLoginAsync()
    {
        if (!vivoxInitialized || VivoxService.Instance.IsLoggedIn) return;

        try
        {
            // 표시 이름 설정 후 로그인
            var options = new LoginOptions()
            {
                DisplayName = AuthManager.Instance.userName
            };

            await VivoxService.Instance.LoginAsync(options);
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 로그인 실패 : {e}");
        }
    }

    /// <summary>
    /// Vivox 로그아웃
    /// </summary>
    public async Task VivoxLogoutAsync()
    {
        if (!VivoxService.Instance.IsLoggedIn) return;

        try
        {
            await VivoxService.Instance.LogoutAsync();
        }
        catch (Exception)
        {
            Debug.LogError("Vivox 로그아웃 실패");
        }
    }

    /// <summary>
    /// 그룹 채널 참여 (무전기용)
    /// </summary>
    public async Task VivoxJoinGroupChannelAsync(string joinCode)
    {
        if (!vivoxInitialized || !VivoxService.Instance.IsLoggedIn) return;

        // 조인코드 기반 채널명 생성
        groupChannelName = joinCode + "_group";

        try
        {
            // 채널 없으면 자동 생성 후 참여
            await VivoxService.Instance.JoinGroupChannelAsync(groupChannelName, ChatCapability.AudioOnly);
            isGroupJoined = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 그룹 채널 참여 실패 {e}");
        }
    }

    /// <summary>
    /// 거리 기반 채널 참여
    /// </summary>
    public async Task VivoxJoinPositionalChannelAsync(string joinCode)
    {
        if (!vivoxInitialized || !VivoxService.Instance.IsLoggedIn) return;

        positionalChannelName = joinCode + "_positional";
        isPositionalJoined = true;

        try
        {
            // 거리 기반 오디오 설정 적용 후 채널 참여
            await VivoxService.Instance.JoinPositionalChannelAsync(
                positionalChannelName,
                ChatCapability.AudioOnly,
                new Channel3DProperties(
                    audibleDistance,              // 최대 거리
                    conversationalDistance,       // 대화 거리
                    audioFadeIntensityByDistanceaudio, // 감쇠 강도
                    AudioFadeModel.InverseByDistance // 거리 기반 감소 방식
                )
            );
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 포지셔널 채널 참여 실패 {e}");
        }
    }

    /// <summary>
    /// 모든 채널 나가기
    /// </summary>
    public async Task VivoxLeaveAllChannelsAsync()
    {
        // 참여 중인 채널만 Leave
        if (isGroupJoined)
            await VivoxService.Instance.LeaveChannelAsync(groupChannelName);

        if (isPositionalJoined)
            await VivoxService.Instance.LeaveChannelAsync(positionalChannelName);

        isGroupJoined = isPositionalJoined = false;
    }

    /// <summary>
    /// 특정 채널 나가기
    /// </summary>
    public async Task VivoxLeaveChannelAsync(string channelName)
    {
        try
        {
            await VivoxService.Instance.LeaveChannelAsync(channelName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Vivox 채널 떠나기 실패 {e}");
        }
    }
}