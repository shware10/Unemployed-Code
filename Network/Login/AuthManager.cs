using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using System;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;
    public event Action OnSignIn;

    public string userName;
    async void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        
        await InitializeUGS();
       
        AuthenticationService.Instance.SignedIn -= OnSdkSignedIn;
        AuthenticationService.Instance.SignedIn += OnSdkSignedIn;
    }

    private void OnSdkSignedIn()
    {
        OnSignIn?.Invoke();
    }
    
    /// <summary>
    ///  UGS 초기화 
    /// </summary>
    private async Task InitializeUGS()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
            Debug.Log("UGS 초기화");
        }
    }
    
    /// <summary>
    /// 회원가입
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public async Task<bool> SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            userName = username;
            Debug.Log("회원가입 성공");
            
            ///vivox 초기화
            await VivoxManager.Instance.InitializeAsync();
            //vivox 로그인
            await VivoxManager.Instance.VivoxLoginAsync();
            
            return true;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }

    /// <summary>
    /// 로그인
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    public async Task<bool> SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            userName = username;
            Debug.Log("로그인 성공");
            
            ///vivox 초기화
            await VivoxManager.Instance.InitializeAsync();
            //vivox 로그인
            await VivoxManager.Instance.VivoxLoginAsync();
            return true;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            return false;
        }
    }
}

