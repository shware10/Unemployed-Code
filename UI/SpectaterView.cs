using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 관전 UI 클래스
/// </summary>
public class SpectaterView : MonoBehaviour
{
    // 관전 대상 플레이어 이름 표시 텍스트
    [SerializeField] private TextMeshProUGUI spectaterPlayerText;

    // 시스템 다운 안내 텍스트
    [SerializeField] private CanvasGroup systemDownTextCg;

    // UIManager 참조
    [SerializeField] private UIManager uiManager;
    
    /// <summary>
    /// 초기화
    /// </summary>
    public void Init()
    {
        GetComponentInParent<PlayerSpectator>().OnSpectatedPlayerChanged += OnSpectatedPlayerChanged;
    }
    
    void OnEnable()
    {
        // 활성화 시 안내 텍스트 페이드 시작
        StartCoroutine(TextRoutine());
    }

    void OnDisable()
    {
        // 비활성화 시 텍스트 상태 초기화
        systemDownTextCg.alpha = 1;
    }

    // 관전 대상이 바뀔 때 UI 갱신
    private void OnSpectatedPlayerChanged(ulong cliendId)
    {
        spectaterPlayerText.SetText($"관전 중 : [ {GameServer.Instance.GetUserName(cliendId)} ]");
    }

    // 일정 시간 후 안내 텍스트 숨김
    private IEnumerator TextRoutine()
    {
        yield return new WaitForSeconds(3f);

        // 페이드 아웃
        StartCoroutine(uiManager.FadeMotion(false, systemDownTextCg));
    }
}
