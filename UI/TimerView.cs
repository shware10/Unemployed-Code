using TMPro;
using UnityEngine;

/// <summary>
/// 타이머 UI 뷰 클래스
/// </summary>
public class TimerView : MonoBehaviour, ITimerListener, IServerStateListener
{
    // UIManager 참조
    private UIManager uiManager;
    
    // 타이머 텍스트
    [SerializeField] private TextMeshProUGUI timerText;

    // 타이머 UI CanvasGroup
    [SerializeField] private CanvasGroup timerCg;

    void Awake()
    {
        // 부모에서 UIManager 가져오기
        uiManager = GetComponentInParent<UIManager>();

        // 초기 텍스트 비우기
        timerText.SetText("");
    }
    
    /// <summary>
    /// 타이머 값 변경 시 호출 메서드
    /// </summary>
    /// <param name="time">남은 시간(초)</param>
    public void OnTimerChanged(int time)
    {
        Debug.Log("타이머 실행중");

        // 30초 남으면 타이머 UI 표시 시작
        if (time == 30)
            StartCoroutine(uiManager.FadeMotion(true, timerCg));
        
        // 시간 표시
        timerText.SetText($"{time}");

        // 0초 > 타이머 숨김
        if (time == 0)
        {
            StartCoroutine(uiManager.FadeMotion(false, timerCg));
            timerText.SetText("");
        }
    }

    /// <summary>
    /// 서버 상태 변경 시 타이머 UI 처리 메서드
    /// </summary>
    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        // 탈출 시작 / 게임 종료 시 즉시 숨김
        if (newState == ServerState.EscapeStart || newState == ServerState.GameOver)
        {
            timerCg.alpha = 0;
        }
    }
}
