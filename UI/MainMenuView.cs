using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인 메뉴 뷰 클래스
/// </summary>
public class MainMenuView : MonoBehaviour
{
    [Header("Panels / Views")]
    // 메인 메뉴 패널
    [SerializeField] private GameObject mainMenuPanel;

    // 로비 화면
    [SerializeField] private GameObject lobbyView;

    // 설정 패널
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    // 시작 버튼
    [SerializeField] private Button startButton;

    // 설정 버튼
    [SerializeField] private Button settingsButton;

    // 종료 버튼
    [SerializeField] private Button exitButton;

    void Start()
    {
        // 버튼 이벤트 연결
        startButton.onClick.AddListener(OnStartGame);
        settingsButton.onClick.AddListener(OnOpenSettings);
        exitButton.onClick.AddListener(OnExitGame);
    }

    // 시작 버튼 > 로비 화면으로 이동
    private void OnStartGame()
    {
        mainMenuPanel.SetActive(false);
        lobbyView.SetActive(true);
    }

    // 설정 버튼 > 설정 화면 표시
    private void OnOpenSettings()
    {
        lobbyView.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // 게임 종료
    private void OnExitGame()
    {
#if UNITY_EDITOR
        // 에디터에서는 플레이 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드에서는 게임 종료
        Application.Quit();
#endif
    }
}