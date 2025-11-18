using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    private Lobby curLobby;
    
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI curPlayersText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;

    [SerializeField] private Button joinButton;

    void Start()
    {
        joinButton = GetComponent<Button>();
        joinButton.onClick.AddListener(OnClickRoom);
    }
    public void Init(Lobby lobby)
    {
        curLobby = lobby;
        roomNameText.SetText(curLobby.Name);
        curPlayersText.SetText($"{curLobby.Players.Count}");
        maxPlayersText.SetText($"{curLobby.MaxPlayers-1}");
    }

    private async void OnClickRoom()
    {
        joinButton.interactable = false;
        if(!await LobbyManager.Instance.JoinByClickAsync(curLobby))
        {
            joinButton.interactable = true;
        }
    }
}
