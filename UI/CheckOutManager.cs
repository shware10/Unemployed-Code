using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 정산결과 UI 뷰들을 관리하는 매니저
/// </summary>
public class CheckOutManager : MonoBehaviour
{
    // 각 플레이어 UI 뷰
    private CheckOutView[] views;
    
    // 표시할 타이틀 노트
    private string[] notes = new string[2]
    {
        "가장 게으른 로봇",
        "가장 부지런한 로봇"
    };

    void Awake()
    {
        // 하위 View들 가져오기
        views = GetComponentsInChildren<CheckOutView>();

        // 초기화
        foreach (CheckOutView view in views)
        {
            view.Init();
        }
    }
        
    void OnEnable()
    {
        // 인덱스 및 비교용 변수 초기화
        int i = -1;

        // 최소 행동 / 최대 점수 추적
        int leastAction = Int32.MaxValue;
        int mostScore = Int32.MinValue;

        int leastActionPlayer = -1;
        int mostScorePlayer = -1;

        // 모든 유저 순회
        foreach (UserData udata in GameServer.Instance.userList)
        {
            int idx = ++i;

            // 최소 행동량 플레이어 찾기
            if (udata.action < leastAction)
            {
                leastAction = udata.action;
                leastActionPlayer = idx;
            }

            // 최대 점수 플레이어 찾기
            if (udata.score > mostScore)
            {
                mostScore = udata.score;
                mostScorePlayer = idx;
            }

            // 기본 정보 표시 (이름 + 생존 여부)
            views[idx].ViewData(udata.username.ToString(), udata.isAlive); 
        }

        // 타이틀 부여
        views[leastActionPlayer].ViewData(notes[0]);
        views[mostScorePlayer].ViewData(notes[1]);
    }

    void OnDisable()
    {
        // 비활성화 시 UI 초기화
        foreach (CheckOutView view in views)
        {
            view.Init();
        }
    }
}