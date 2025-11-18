using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [Header("Panels / Views")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject lobbyView;     
    [SerializeField] private GameObject settingsPanel;  

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    // 설정창 단순 보여주기식 세팅만 일단 진행.

    void Start()
    {
        startButton.onClick.AddListener(OnStartGame);
        settingsButton.onClick.AddListener(OnOpenSettings);
        exitButton.onClick.AddListener(OnExitGame);
    }

    private void OnStartGame()
    {
        mainMenuPanel.SetActive(false);
        lobbyView.SetActive(true);
    }

    private void OnOpenSettings()
    {
        lobbyView.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}