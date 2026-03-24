using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// 로그인 / 회원가입 UI 컨트롤러
/// - 입력값 처리
/// - AuthManager 호출
/// - 성공 시 씬 전환
/// </summary>
public class AuthView : MonoBehaviour
{
    [Header("CanvasGroup")] 
    [SerializeField] private GameObject logInPanel;
    [SerializeField] private GameObject signUpPanel;
    
    [Header("SignIn")]
    [SerializeField] private TMP_InputField i_idText;
    [SerializeField] private TMP_InputField i_passwordText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signUpButton;
    
    [Header("SignUp")]
    [SerializeField] private TMP_InputField u_idText;
    [SerializeField] private TMP_InputField u_passwordText;
    [SerializeField] private TMP_InputField u_passwordCheckText;
    [SerializeField] private Button createButton;
    [SerializeField] private Button cancelButton;

    [Header("NextScene")]
    [SerializeField] private string nextSceneName = "MainScene";

    void Start()
    {
        // 로그인 성공 이벤트 등록 (중복 방지 후 재등록)
        AuthManager.Instance.OnSignIn -= OnSuccess;
        AuthManager.Instance.OnSignIn += OnSuccess;
        
        // 버튼 이벤트 연결
        loginButton.onClick.AddListener(OnClickLoginButton);
        signUpButton.onClick.AddListener(() => ShowPanel(logInPanel, signUpPanel));

        createButton.onClick.AddListener(OnClickCreateButton);
        cancelButton.onClick.AddListener(() => ShowPanel(signUpPanel, logInPanel));
    }

    /// <summary>
    /// 로그인 버튼 클릭
    /// </summary>
    async void OnClickLoginButton()
    {
        // 중복 클릭 방지
        loginButton.interactable = false; 
        
        // 입력값 정리 (공백/특수문자 제거 등)
        string username = StringCleaner.Clean(i_idText.text);
        string password = StringCleaner.Clean(i_passwordText.text);
        
        // 로그인 요청
        bool ok = await AuthManager.Instance.SignInWithUsernamePasswordAsync(username, password);

        // 실패 시 버튼 다시 활성화
        if (!ok) loginButton.interactable = true;
    }

    /// <summary>
    /// 회원가입 버튼 클릭
    /// </summary>
    async void OnClickCreateButton()
    {
        // 비밀번호 확인 체크
        if (u_passwordText.text != u_passwordCheckText.text) return;

        // 중복 클릭 방지
        createButton.interactable = false;

        // 입력값 정리
        string username = StringCleaner.Clean(u_idText.text);
        string password = StringCleaner.Clean(u_passwordCheckText.text);
        
        // 회원가입 요청
        bool ok = await AuthManager.Instance.SignUpWithUsernamePasswordAsync(username, password);

        if (ok)
        {
            // 성공 시 로그인 패널로 전환
            ShowPanel(signUpPanel, logInPanel);
        }
        else
        {
            // 실패 시 버튼 복구
            createButton.interactable = true;
        }
    }

    /// <summary>
    /// 로그인/회원가입 패널 전환 
    /// </summary>
    void ShowPanel(GameObject curPanel, GameObject nextPanel)
    {
        curPanel.SetActive(false);
        nextPanel.SetActive(true);
    }

    /// <summary>
    /// 로그인 성공 시 호출 > 다음 씬 이동
    /// </summary>
    void OnSuccess()
    {
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
