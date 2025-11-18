using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

public class CheckOutManager : MonoBehaviour
{
    private CheckOutView[] views;
    
    private string[] notes = new string[2]
    {
        "가장 게으른 로봇",
        "가장 부지런한 로봇"
    };

    void Awake()
    {
        views = GetComponentsInChildren<CheckOutView>();
        foreach (CheckOutView view in views)
        {
            view.Init();
        }
    }
        
    void OnEnable()
    {
        int i = -1;
        int leastAction = Int32.MaxValue;
        int mostScore = Int32.MinValue;
        int leastActionPlayer = -1;
        int mostScorePlayer = -1;
        foreach (UserData udata in GameServer.Instance.userList)
        {
            int idx = ++i;
            if (udata.action < leastAction)
            {
                leastAction = udata.action;
                leastActionPlayer = idx;
            }
            if (udata.score > mostScore)
            {
                mostScore = udata.score;
                mostScorePlayer = idx;
            }
            views[idx].ViewData(udata.username.ToString(), udata.isAlive); 
        }
        views[leastActionPlayer].ViewData(notes[0]);
        views[mostScorePlayer].ViewData(notes[1]);
    }

    void OnDisable()
    {
        foreach (CheckOutView view in views)
        {
            view.Init();
        }
    }
}
