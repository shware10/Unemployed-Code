using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 맵 정보를 보여주는 뷰 클래스
/// </summary>
public class MapInfoView : MonoBehaviour, IServerStateListener
{
    // UIManager 참조
    [SerializeField] private UIManager uiManager;

    // 맵 이름 텍스트
    [SerializeField] private TextMeshProUGUI mapInfoText;

    // 페이딩용 CanvasGroup
    [SerializeField] private CanvasGroup mapInfoCg;
    
    // 표시 유지 시간
    private WaitForSeconds duration = new WaitForSeconds(3f);

    // 맵 이름 표시  > 페이드 인 > 유지 > 페이드 아웃
    private IEnumerator FadeRoutine(string mapName)
    {
        // 세션 시작 후 약간의 딜레이
        yield return new WaitForSeconds(10f);
        
        // 맵 이름 설정
        mapInfoText.SetText($"{mapName}");
        
        // 페이드 인
        yield return uiManager.FadeMotion(true, mapInfoCg);

        // 일정 시간 유지
        yield return duration;

        // 페이드 아웃
        yield return uiManager.FadeMotion(false, mapInfoCg);
    }

    /// <summary>
    /// 서버 상태 변경 시 맵 정보 표시
    /// </summary>
    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        switch (newState)
        {
            case ServerState.SessionStart:

                // 현재 씬 이름 확인
                Scene curScene = SceneManager.GetActiveScene();

                // 씬 이름에 따라 맵 정보 표시
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