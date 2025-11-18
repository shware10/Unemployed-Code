using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapInfoView : MonoBehaviour,IServerStateListener
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TextMeshProUGUI mapInfoText;
    [SerializeField] private CanvasGroup mapInfoCg;
    
    private WaitForSeconds duration = new WaitForSeconds(3f);

    IEnumerator FadeRoutine(string mapName)
    {
        yield return new WaitForSeconds(10f);
        
        mapInfoText.SetText($"{mapName}");
        
        yield return uiManager.FadeMotion(true, mapInfoCg);
        yield return duration;
        yield return uiManager.FadeMotion(false, mapInfoCg);
    }

    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        switch (newState)
        {
            case ServerState.SessionStart:
                Scene curScene = SceneManager.GetActiveScene();
                if (curScene.name == "IngameScene_Subway-2(Farming)")
                {
                    StartCoroutine(FadeRoutine("2번 정거장"));
                }
                else if (curScene.name == "IngameScene_Subway-3(Farming)")
                {
                    StartCoroutine(FadeRoutine("3번 정거장"));
                }
                else if (curScene.name == "IngameScene_Subway-4(Farming)")
                {
                    StartCoroutine(FadeRoutine("4번 정거장"));
                }
                break;
        }
    }
}
