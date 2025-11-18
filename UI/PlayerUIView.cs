using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIView : NetworkBehaviour, IPlayerListener
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private Image batteryFill;
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private Image netIcon;
    [SerializeField] private Image flashIcon;
    [SerializeField] private Image radioIcon;
    [SerializeField] private Image stunIcon;

    public override void OnNetworkSpawn()
    {
        Init();
    }
    public void GetBattery(float maxBattery, float curBattery)
    {
        float percent = curBattery / maxBattery;
        batteryFill.fillAmount = percent;
        batteryText.SetText($"{(int)(percent * 100)}%");
        Debug.Log($"남은 배터리 : {curBattery} / {maxBattery}");
    }

    private void Init()
    {
        flashIcon.gameObject.SetActive(false);
        radioIcon.gameObject.SetActive(false);
        netIcon.gameObject.SetActive(false);
        stunIcon.gameObject.SetActive(false);
    }
    
    public void OnDead()
    {
        if (IsOwner)
        {
            uiManager.SystemDown();
            GameServer.Instance.DieServerRpc();
        }
    }

    public void OnRespawn()
    {
        if (IsOwner)
        {
            if (uiManager.curCg == uiManager.systemDownCg)
            {
                uiManager.SystemActivate();                
            }
        }
    }

    public void FlashLightState(bool isOn)
    {
        flashIcon.gameObject.SetActive(isOn);
        Debug.Log($"손전등 On/Off : {isOn}");
    }

    public void RadioState(bool isOn)
    {
        radioIcon.gameObject.SetActive(isOn);
        VivoxManager.Instance.SwitchChannelAsync(isOn);
    }

    public void NetState(bool isOn)
    {
        netIcon.gameObject.SetActive(isOn);
        Debug.Log($"Net On/Off : {isOn}");
    }

    public void StunState(bool isOn)
    {
        stunIcon.gameObject.SetActive(isOn);
        Debug.Log($"Stun On/Off : {isOn}");
    }

}
