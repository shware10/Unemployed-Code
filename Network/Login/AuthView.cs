using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Text;
using System.Text.RegularExpressions;

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

    [Header("NextScene")] [SerializeField] private string nextSceneName = "MainScene";

    void Start()
    {
        AuthManager.Instance.OnSignIn -= OnSuccess;
        AuthManager.Instance.OnSignIn += OnSuccess;
        
        loginButton.onClick.AddListener(OnClickLoginButton);
        signUpButton.onClick.AddListener(() => ShowPanel(logInPanel, signUpPanel));

        createButton.onClick.AddListener(OnClickCreateButton);
        cancelButton.onClick.AddListener(() => ShowPanel(signUpPanel, logInPanel));
    }

    async void OnClickLoginButton()
    {
        loginButton.interactable = false; //중복 클릭 방지
        
        string username = StringCleaner.Clean(i_idText.text);
        string password = StringCleaner.Clean(i_passwordText.text);
        
        bool ok = await AuthManager.Instance.SignInWithUsernamePasswordAsync(username, password);

        if (!ok) loginButton.interactable = true;
    }

    async void OnClickCreateButton()
    {
        if (u_passwordText.text != u_passwordCheckText.text) return;

        createButton.interactable = false; //중복 클릭 방지

        string username = StringCleaner.Clean(u_idText.text);
        string password = StringCleaner.Clean(u_passwordCheckText.text);
        
        bool ok = await AuthManager.Instance.SignUpWithUsernamePasswordAsync(username, password);
        if (ok) ShowPanel(signUpPanel, logInPanel);
        else createButton.interactable = true;
    }

    void ShowPanel(GameObject curPanel, GameObject nextPanel)
    {
        curPanel.SetActive(false);
        nextPanel.SetActive(true);
    }

    void OnSuccess()
    {
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }

}
