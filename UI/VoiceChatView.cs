using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Vivox;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Vivox 참가자 목록 표시 UI 클래스
/// </summary>
public class VoiceChatView : NetworkBehaviour, IParticipationListener
{
    // 참가자 리스트 UI 프리팹
    [SerializeField] private GameObject chatPrefab;

    // UI 부모
    [SerializeField] private Transform chatParent;

    // DisplayName Chat UI 매핑
    private Dictionary<string, Chat> chatDict = new Dictionary<string, Chat>();

    public override void OnNetworkSpawn()
    {
        // 내 플레이어만 이벤트 구독
        if (IsOwner)
        {
            VivoxManager.Instance.OnParticipantChangedEvent += OnParticipantChanged;    
        }
    }

    public override void OnNetworkDespawn()
    {
        // 이벤트 해제
        if (IsOwner)
        {
            VivoxManager.Instance.OnParticipantChangedEvent -= OnParticipantChanged;            
        }
    }

    // 기존 음성채팅 리스트 UI 제거
    private void CleanChat()
    {
        foreach (Transform child in chatParent)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 참가자 목록 변경 시 호출
    /// </summary>
    public void OnParticipantChanged(List<VivoxParticipant> participants)
    {
        StartCoroutine(OnParticipantChangedRoutine(participants));
    }

    // UI 생성 + 말하기 이벤트 연결
    private IEnumerator OnParticipantChangedRoutine(List<VivoxParticipant> participants)
    {
        // 기존 UI 초기화
        CleanChat();

        // 참가자 UI 생성
        foreach (VivoxParticipant participant in participants)
        {
            // 거리 채팅 채널만 표시
            if (participant.ChannelName.EndsWith("_positional"))
            {
                GameObject chatObj = Instantiate(chatPrefab, chatParent);

                // 이름 표시
                TextMeshProUGUI chatText = chatObj.GetComponent<TextMeshProUGUI>();
                chatText.SetText(participant.DisplayName);

                // Chat 컴포넌트 저장
                Chat chat = chatObj.GetComponent<Chat>();
                chatDict[participant.DisplayName] = chat;
            }
        }

        // Vivox 상태 동기화 대기
        yield return new WaitForSeconds(2f);
        
        // 말하기 감지 이벤트 연결
        foreach (VivoxParticipant participant in participants)
        {
            participant.ParticipantSpeechDetected += () =>
            {
                if (participant.SpeechDetected)
                {
                    chatDict[participant.DisplayName].ActiveImage(true);                    
                }
                else
                {
                    chatDict[participant.DisplayName].ActiveImage(false);     
                }
            };
        }
    }
}
