using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;
using System;

/// <summary>
/// UGS인증 + Vivox 초기화 관리 싱글톤
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    /// <summary> 로그인 완료 이벤트 </summary>
    public event Action OnSignIn;

    /// <summary> 현재 로그인한 유저 이름 </summary>
    public string userName;

    async void Awake()
    {
        // 싱글톤 설정
        Instance = this;
        DontDestroyOnLoad(this);
        
        // UGS 초기화
        await InitializeUGS();

        // 로그인 완료 이벤트 등록 (중복 방지 후 재등록)
        AuthenticationService.Instance.SignedIn -= OnSdkSignedIn;
        AuthenticationService.Instance.SignedIn += OnSdkSignedIn;
    }

    private void OnSdkSignedIn()
    {
        // SDK 로그인 완료 시 외부에 알림
        OnSignIn?.Invoke();
    }
    
    /// <summary>
    /// UGS 초기화 (한 번만 실행)
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
    /// 회원가입 + Vivox 초기화/로그인
    /// </summary>
    public async Task<bool> SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            // UGS 회원가입
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            userName = username;

            // Vivox 준비 (음성채팅)
            await VivoxManager.Instance.InitializeAsync();
            await VivoxManager.Instance.VivoxLoginAsync();
            
            return true;
        }
        catch (AuthenticationException ex)
        {
            // 인증 관련 오류
            Debug.LogException(ex);
            return false;
        }
        catch (RequestFailedException ex)
        {
            // 네트워크 / 요청 실패
            Debug.LogException(ex);
            return false;
        }
    }

    /// <summary>
    /// 로그인 + Vivox 초기화/로그인
    /// </summary>
    public async Task<bool> SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            // UGS 로그인
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            userName = username;

            // Vivox 준비 (음성채팅)
            await VivoxManager.Instance.InitializeAsync();
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