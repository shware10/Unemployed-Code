using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour, ITimerListener, IServerStateListener
{
    private UIManager uiManager;
    
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private CanvasGroup timerCg;

    void Awake()
    {
        uiManager = GetComponentInParent<UIManager>();
        timerText.SetText("");
    }
    
    public void OnTimerChanged(int time)
    {
        Debug.Log("타이머 실행중");
        if (time == 30) StartCoroutine(uiManager.FadeMotion(true, timerCg));
        
        timerText.SetText($"{time}");

        if (time == 0)
        {
            StartCoroutine(uiManager.FadeMotion(false, timerCg));
            timerText.SetText("");
        }
    }

    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        if (newState == ServerState.EscapeStart || newState == ServerState.GameOver)
        {
            timerCg.alpha = 0;
        }
    }
}
