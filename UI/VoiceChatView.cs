using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Vivox;
using Unity.VisualScripting;
using UnityEngine;

public class VoiceChatView : NetworkBehaviour, IParticipationListener
{
    [SerializeField] private GameObject chatPrefab;
    [SerializeField] private Transform chatParent;

    private Dictionary<string,Chat> chatDict = new Dictionary<string,Chat>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            VivoxManager.Instance.OnParticipantChangedEvent += OnParticipantChanged;    
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            VivoxManager.Instance.OnParticipantChangedEvent -= OnParticipantChanged;            
        }
    }

    private void CleanChat()
    {
        foreach (Transform child in chatParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnParticipantChanged(List<VivoxParticipant> participants)
    {
        StartCoroutine(OnParticipantChangedRoutine(participants));
    }

    IEnumerator OnParticipantChangedRoutine(List<VivoxParticipant> participants)
    {
        CleanChat();
        foreach (VivoxParticipant participant in participants)
        {
            if (participant.ChannelName.EndsWith("_positional"))
            {
                GameObject chatObj = Instantiate(chatPrefab, chatParent);
                TextMeshProUGUI chatText = chatObj.GetComponent<TextMeshProUGUI>();
                Chat chat = chatObj.GetComponent<Chat>();
                chatText.SetText(participant.DisplayName);
                chatDict[participant.DisplayName] = chat;
            }
        }

        yield return new WaitForSeconds(2f);
        
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
