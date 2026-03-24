using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 로비 UI 컨트롤러
/// - 로비 리스트 표시
/// - 방 생성 / 참가 / 검색
/// - 패널 전환 관리
/// </summary>
public class LobbyView : MonoBehaviour
{
    [Header("Room")]
    private List<Lobby> lobbyList;

    // 로비 리스트 패널
    [SerializeField] private GameObject listPanel;

    // 방 UI 프리팹
    [SerializeField] private GameObject roomPrefab;

    // 방 UI 생성 부모
    [SerializeField] private Transform parent;
    
    [Header("Refresh")]
    [SerializeField] private Button refreshButton;
    
    [Header("Create")]
    [SerializeField] private GameObject createPanel;
    
    [SerializeField] private Button createTab;
    [SerializeField] private Button createButton;
    [SerializeField] private Button createCancelButton;

    // 생성할 방 이름 입력필드
    [SerializeField] private TMP_InputField lobbyNameText;

    // 비공개 방 여부 토글
    [SerializeField] private Toggle privateToggle;
    
    [Header("Search")]
    [SerializeField] private GameObject searchPanel;
    
    [SerializeField] private Button searchTab;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button joinCancelButton;

    // 조인 코드 입력필드
    [SerializeField] private TMP_InputField joinCodeText; 

    void Start()
    {
        // Refresh 버튼 이벤트 등록
        refreshButton.onClick.AddListener(ReBuildList);
        refreshButton.onClick.AddListener(() => ShowPanel(listPanel));
        
        // Create 버튼 이벤트 등록
        createTab.onClick.AddListener(() => ShowPanel(createPanel));
        createButton.onClick.AddListener(OnCreateButtonClick);

        // Cancel 버튼 이벤트 등록
        createCancelButton.onClick.AddListener(ReBuildList);
        createCancelButton.onClick.AddListener(() => ShowPanel(listPanel));
        
        // Search 버튼 이벤트 등록
        searchTab.onClick.AddListener(() => ShowPanel(searchPanel));
        joinButton.onClick.AddListener(OnJoinButtonClick);

        // Join 버튼 이벤트 등록
        joinCancelButton.onClick.AddListener(ReBuildList);
        joinCancelButton.onClick.AddListener(() => ShowPanel(listPanel));
        
        // 첫 진입 시 로비 목록 불러오기
        ReBuildList();
    }

    /// <summary>
    /// 패널 전환
    /// </summary>
    private void ShowPanel(GameObject curPanel)
    {
        listPanel.SetActive(listPanel == curPanel);
        searchPanel.SetActive(searchPanel == curPanel);
        createPanel.SetActive(createPanel == curPanel);
    }

    /// <summary>
    /// 코드로 로비 참가
    /// </summary>
    private async void OnJoinButtonClick()
    {
        joinButton.interactable = false; // 중복 클릭 방지

        // 조인 코드 기반 참가 요청
        if (!await LobbyManager.Instance.JoinByCodeAsync(joinCodeText.text))
        {
            // 실패 시 버튼 복구
            joinButton.interactable = true;
        }
    }

    /// <summary>
    /// 방 생성 요청
    /// </summary>
    private async void OnCreateButtonClick()
    {
        createButton.interactable = false; // 중복 클릭 방지

        // 방 생성 (이름, 최대 인원, 비공개 여부)
        if (!await LobbyManager.Instance.CreateRoomAsync(lobbyNameText.text, 5, privateToggle.isOn))
        {
            // 실패 시 버튼 복구
            createButton.interactable = true;
        }
    } 
    
    /// <summary>
    /// 로비 리스트 재생성
    /// </summary>
    private async void ReBuildList()
    {
        refreshButton.interactable = false; // 중복 요청 방지

        // 기존 방 UI 제거
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
        
        // 서버에서 최신 로비 목록 받아오기
        lobbyList = await LobbyManager.Instance.RefreshLobbiesAsync();
        
        // 로비 개수 확인
        Debug.Log($"{lobbyList.Count}");
        
        // 각 로비마다 UI 생성
        foreach (Lobby lobby in lobbyList)
        {
            GameObject roomObj = Instantiate(roomPrefab, parent);

            // Room 컴포넌트에 데이터 바인딩
            Room room = roomObj.GetComponent<Room>();
            room.Init(lobby);
        }

        Debug.Log("로비 생성 완료");

        refreshButton.interactable = true; // 버튼 복구
    }
}
