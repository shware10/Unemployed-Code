using System.Collections;
using System.Xml.Serialization;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;

/// <summary>
/// 플레이어 프리팹에 넣어야 하는 클래스
/// </summary>
public class VivoxPositionUpdater : NetworkBehaviour
{
    [SerializeField] Transform listener; //플레이어 카메라
    [SerializeField] private WaitForSeconds interval = new WaitForSeconds(0.1f);
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            StartCoroutine(WaitAndStartVoicePosition());
        }
    }

    private IEnumerator WaitAndStartVoicePosition()
    {
        // Vivox 초기화/로그인/채널조인이 끝날 때까지 대기
        while (VivoxManager.Instance == null || string.IsNullOrEmpty(VivoxManager.Instance.positionalChannelName))
        {
            yield return null;
        }

        yield return new WaitForSeconds(3f);
        
        // 모든 준비가 끝나면 음성 위치 업데이트 시작
        yield return StartCoroutine(UpdateVoicePosition());
    }
    
    /// <summary>
    /// 3d 채널상의 위치를 최신화하는 함수입니다.
    /// </summary>
    IEnumerator UpdateVoicePosition()
    {
        if (IsOwner)
        {
            while(true)
            {
                VivoxService.Instance.Set3DPosition(
                    transform.position,
                    listener.position,
                    listener.forward,
                    listener.up,
                    VivoxManager.Instance.positionalChannelName
                );

                yield return interval;
            }
        }
    }
}
