using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 알림 UI 뷰 클래스
/// </summary>
public class AlarmView : MonoBehaviour, IConnectionListener, IServerStateListener
{
    // 알림 텍스트 프리팹
    [SerializeField] private GameObject alarmPrefab;

    // 알림 UI 부모
    [SerializeField] private Transform alarmParent;

    // 스크롤 영역
    [SerializeField] private ScrollRect alarmScrollRect;

    // 생성된 알림 저장
    [SerializeField] private Queue<GameObject> alarmQueue = new Queue<GameObject>();

    // 알림 UI 생성
    private TextMeshProUGUI CreateAlarm()
    {
        GameObject alarm = Instantiate(alarmPrefab, alarmParent);

        // 큐에 저장
        alarmQueue.Enqueue(alarm);

        // 텍스트 컴포넌트 가져오기
        TextMeshProUGUI alarmText = alarm.GetComponent<TextMeshProUGUI>();

        return alarmText;
    }
    
    /// <summary>
    /// 유저 접속 / 퇴장 시 호출
    /// </summary>
    public void OnConnctionChanged(FixedString64Bytes userName, bool isConnected)
    {
        TextMeshProUGUI alarmText = CreateAlarm();

        // 스크롤 맨 아래로 이동
        ScrollToBottom();
        
        // 메시지 출력
        if (isConnected)
            alarmText.SetText($"{userName}이(가) 연결되었습니다.");
        else
            alarmText.SetText($"{userName}이(가) 떠났습니다.");
    }

    /// <summary>
    /// 서버 상태 변경 시 호출
    /// </summary>
    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        TextMeshProUGUI alarmText = CreateAlarm();

        switch (newState)
        {
            case ServerState.GameStart:
                alarmText.SetText("게임이 시작됩니다.");
                break;

            case ServerState.SessionStart:
                alarmText.SetText("세션이 시작됩니다.");
                break;

            case ServerState.SessionEnd:
                alarmText.SetText("세션종료.");
                break;

            case ServerState.GameOver:
                alarmText.SetText("게임종료.");
                break;
        }
    }
    
    /// <summary>
    /// 스크롤을 맨 아래로 이동
    /// </summary>
    public void ScrollToBottom()
    {
        // 0 = 맨 아래
        alarmScrollRect.verticalNormalizedPosition = 0f;
        
        // 레이아웃 강제 갱신
        Canvas.ForceUpdateCanvases();
    }
}
