using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyView : MonoBehaviour
{
    [Header("Room")]
    private List<Lobby> lobbyList;
    [SerializeField] private GameObject listPanel;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Transform parent;
    
    [Header("Refresh")]
    [SerializeField] private Button refreshButton;
    
    [Header("Create")]
    [SerializeField] private GameObject createPanel;
    
    [SerializeField] private Button createTab;
    [SerializeField] private Button createButton;
    [SerializeField] private Button createCancelButton;

    [SerializeField] private TMP_InputField lobbyNameText;

    [SerializeField] private Toggle privateToggle;
    
    [Header("Search")]
    [SerializeField] private GameObject searchPanel;
    
    [SerializeField] private Button searchTab;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button joinCancelButton;

    [SerializeField] private TMP_InputField joinCodeText; 
    void Start()
    {
        //Refresh
        refreshButton.onClick.AddListener(ReBuildList);
        refreshButton.onClick.AddListener(() => ShowPanel(listPanel));
        
        //Create
        createTab.onClick.AddListener(() => ShowPanel(createPanel));
        createButton.onClick.AddListener(OnCreateButtonClick);
        createCancelButton.onClick.AddListener(ReBuildList);
        createCancelButton.onClick.AddListener(() => ShowPanel(listPanel));
        
        //Search
        searchTab.onClick.AddListener(() => ShowPanel(searchPanel));
        joinButton.onClick.AddListener(OnJoinButtonClick);
        joinCancelButton.onClick.AddListener(ReBuildList);
        joinCancelButton.onClick.AddListener(() => ShowPanel(listPanel));
        
        //첫 접속 로비 불러오기
        ReBuildList();
    }

    private void ShowPanel(GameObject curPanel)
    {
        listPanel.SetActive(listPanel == curPanel);
        searchPanel.SetActive(searchPanel == curPanel);
        createPanel.SetActive(createPanel == curPanel);
    }

    private async void OnJoinButtonClick()
    {
        joinButton.interactable = false;
        if (!await LobbyManager.Instance.JoinByCodeAsync(joinCodeText.text))
        {
            joinButton.interactable = true;
        }
    }

    private async void OnCreateButtonClick()
    {
        createButton.interactable = false;
        if (!await LobbyManager.Instance.CreateRoomAsync(lobbyNameText.text, 5, privateToggle.isOn))
        {
            createButton.interactable = true;
        }
    } 
    
    
    /// <summary>
    /// 룸을 재생성하는 함수
    /// </summary>
    private async void ReBuildList()
    {
        refreshButton.interactable = false;
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
        
        lobbyList = await LobbyManager.Instance.RefreshLobbiesAsync();
        
        Debug.Log($"{lobbyList.Count}");
        
        foreach(Lobby lobby in lobbyList)
        {
            Debug.Log(lobby.Name);
            GameObject roomObj = Instantiate(roomPrefab, parent);
            Room room = roomObj.GetComponent<Room>();
            room.Init(lobby);
        }
        Debug.Log("로비 생성 완료");
        refreshButton.interactable = true;
    }
}
