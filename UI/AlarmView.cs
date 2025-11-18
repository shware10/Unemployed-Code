using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlarmView : MonoBehaviour,IConnectionListener,IServerStateListener
{
    [SerializeField] private GameObject alarmPrefab;
    [SerializeField] private Transform alarmParent;
    [SerializeField] private ScrollRect alarmScrollRect;

    [SerializeField] private Queue<GameObject> alarmQueue = new Queue<GameObject>();

    private TextMeshProUGUI CreateAlarm()
    {
        GameObject alarm = Instantiate(alarmPrefab, alarmParent);
        alarmQueue.Enqueue(alarm);

        TextMeshProUGUI alarmText = alarm.GetComponent<TextMeshProUGUI>();

        return alarmText;
    }
    
    public void OnConnctionChanged(FixedString64Bytes userName, bool isConnected)
    {
        TextMeshProUGUI alarmText = CreateAlarm();

        ScrollToBottom();
        
        if (isConnected) alarmText.SetText($"{userName}이(가) 연결되었습니다.");
        else             alarmText.SetText($"{userName}이(가) 떠났습니다.");
    }

    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        TextMeshProUGUI alarmText = CreateAlarm();
        switch (newState)
        {
            case ServerState.GameStart :
                alarmText.SetText("게임이 시작됩니다.");
                break;
            case ServerState.SessionStart :
                alarmText.SetText("세션이 시작됩니다.");
                break;
            case ServerState.SessionEnd :
                alarmText.SetText("세션종료.");
                break;
            case ServerState.GameOver :
                alarmText.SetText("게임종료.");
                break;
        }
    }
    
    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases(); // 레이아웃 갱신 강제
        alarmScrollRect.verticalNormalizedPosition = 0f; // 0f = 맨 아래 / 1f = 맨 위
    }
}
