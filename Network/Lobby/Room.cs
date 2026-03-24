using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 목록에서 하나의 방(UI)을 표현하는 클래스
/// </summary>
public class Room : MonoBehaviour
{
    /// <summary> 현재 바인딩된 로비 데이터 </summary>
    private Lobby curLobby;
    
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI curPlayersText;
    [SerializeField] private TextMeshProUGUI maxPlayersText;

    [SerializeField] private Button joinButton;

    void Start()
    {
        // 버튼 컴포넌트 가져와서 클릭 이벤트 등록
        joinButton = GetComponent<Button>();
        joinButton.onClick.AddListener(OnClickRoom);
    }

    /// <summary>
    /// 로비 데이터 바인딩 + UI 갱신
    /// </summary>
    public void Init(Lobby lobby)
    {
        curLobby = lobby;

        // 방 이름 표시
        roomNameText.SetText(curLobby.Name);

        // 현재 인원 표시
        curPlayersText.SetText($"{curLobby.Players.Count}");

        // 최대 인원 표시 (호스트 제외로 -1)
        maxPlayersText.SetText($"{curLobby.MaxPlayers - 1}");
    }

    /// <summary>
    /// 방 클릭 시 호출 > 로비 참가 시도
    /// </summary>
    private async void OnClickRoom()
    {
        // 중복 클릭 방지
        joinButton.interactable = false;

        // 로비 참가 요청
        if (!await LobbyManager.Instance.JoinByClickAsync(curLobby))
        {
            // 실패 시 버튼 다시 활성화
            joinButton.interactable = true;
        }
    }
}
