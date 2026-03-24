using System.Collections;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 네트워크 바인더 클래스
/// </summary>
public class PlayerBinder : NetworkBehaviour
{
    [SerializeField] private UIManager uiManager;

    public override void OnNetworkSpawn()
    {
        // 내 플레이어일 때만 실행
        if (IsOwner)
        {
            StartCoroutine(DelaySendData());
        }
    }
    
    IEnumerator DelaySendData()
    {
        // 초기화 타이밍 맞추기 위해 1프레임 대기
        yield return null;
        
        // 서버에 플레이어 등록
        GameServer.Instance.BindServer(gameObject);

        // 유저 이름 서버로 전송
        GameServer.Instance.SendDataServerRpc(AuthManager.Instance.userName);

        // UI 초기화
        uiManager.Init();

        // 씬 로드 시 UI 카메라 재설정
        SceneManager.sceneLoaded += uiManager.GetUICamera;
    }
}