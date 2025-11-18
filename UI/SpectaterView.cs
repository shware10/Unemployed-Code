using System.Collections;
using TMPro;
using UnityEngine;

public class SpectaterView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI spectaterPlayerText;
    [SerializeField] private CanvasGroup systemDownTextCg;
    [SerializeField] private UIManager uiManager;
    
    public void Init()
    {
        GetComponentInParent<PlayerSpectator>().OnSpectatedPlayerChanged += OnSpectatedPlayerChanged;
    }
    
    void OnEnable()
    {
        StartCoroutine(TextRoutine());
    }

    void OnDisable()
    {
        systemDownTextCg.alpha = 1;
    }

    void OnSpectatedPlayerChanged(ulong cliendId)
    {
        spectaterPlayerText.SetText($"관전 중 : [ {GameServer.Instance.GetUserName(cliendId)} ]");
    }

    IEnumerator TextRoutine()
    {
        yield return new WaitForSeconds(3f);
        uiManager.FadeMotion(false, systemDownTextCg);
    }
}
