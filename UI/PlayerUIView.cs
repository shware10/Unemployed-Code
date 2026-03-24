using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 HUD 뷰 클래스
/// </summary>
public class PlayerUIView : NetworkBehaviour, IPlayerListener
{
    // UIManager 참조
    [SerializeField] private UIManager uiManager;

    // 배터리 UI
    [SerializeField] private Image batteryFill;
    [SerializeField] private TextMeshProUGUI batteryText;

    // 상태 아이콘들
    [SerializeField] private Image netIcon;
    [SerializeField] private Image flashIcon;
    [SerializeField] private Image radioIcon;
    [SerializeField] private Image stunIcon;

    public override void OnNetworkSpawn()
    {
        // 네트워크 스폰 시 UI 초기화
        Init();
    }

    /// <summary>
    /// 배터리 상태 UI 갱신
    /// </summary>
    /// <param name="maxBattery">최대 배터리</param>
    /// <param name="curBattery">현재 배터리</param>
    public void GetBattery(float maxBattery, float curBattery)
    {
        float percent = curBattery / maxBattery;

        // 게이지 업데이트
        batteryFill.fillAmount = percent;

        // 퍼센트 텍스트 표시
        batteryText.SetText($"{(int)(percent * 100)}%");

        Debug.Log($"남은 배터리 : {curBattery} / {maxBattery}");
    }

    // 초기 상태 (아이콘 숨김)
    private void Init()
    {
        flashIcon.gameObject.SetActive(false);
        radioIcon.gameObject.SetActive(false);
        netIcon.gameObject.SetActive(false);
        stunIcon.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 플레이어 사망 시 처리
    /// </summary>
    public void OnDead()
    {
        if (IsOwner)
        {
            // UI 변경 (다운 상태)
            uiManager.SystemDown();

            // 서버에 사망 전달
            GameServer.Instance.DieServerRpc();
        }
    }

    /// <summary>
    /// 플레이어 부활 시 처리
    /// </summary>
    public void OnRespawn()
    {
        if (IsOwner)
        {
            // 다운 상태일 때만 UI 복구
            if (uiManager.curCg == uiManager.systemDownCg)
            {
                uiManager.SystemActivate();                
            }
        }
    }

    /// <summary>
    /// 손전등 상태 아이콘 활성/비활성
    /// </summary>
    public void FlashLightState(bool isOn)
    {
        flashIcon.gameObject.SetActive(isOn);
        Debug.Log($"손전등 On/Off : {isOn}");
    }

    /// <summary>
    /// 무전기 상태 아이콘 활성/비활성 + 음성 채널 전환
    /// </summary>
    public void RadioState(bool isOn)
    {
        radioIcon.gameObject.SetActive(isOn);

        // 무전기 ON > 그룹 채널, OFF > 거리 채널
        VivoxManager.Instance.SwitchChannelAsync(isOn);
    }

    /// <summary>
    /// 그물 상태 아이콘 활성/비활성
    /// </summary>
    public void NetState(bool isOn)
    {
        netIcon.gameObject.SetActive(isOn);
        Debug.Log($"Net On/Off : {isOn}");
    }

    /// <summary>
    /// 기절 상태 아이콘 활성/비활성
    /// </summary>
    public void StunState(bool isOn)
    {
        stunIcon.gameObject.SetActive(isOn);
        Debug.Log($"Stun On/Off : {isOn}");
    }
}
