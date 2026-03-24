using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 UI 전체 관리 클래스
/// </summary>
public class UIManager : MonoBehaviour, IServerStateListener
{
    // 페이드 지속 시간
    [SerializeField] private float fadeDuration = 0.5f;

    // 관전자 UI
    [SerializeField] private SpectaterView spectaterView;
    
    public CanvasGroup systemActivateCg;
    public CanvasGroup systemDownCg;
    public CanvasGroup checkOutCg;
    public CanvasGroup systemEnvCg;
    public CanvasGroup gameOverCg;

    // 현재 활성화된 UI
    public CanvasGroup curCg;
    
    /// <summary>
    /// 서버 상태 변경 시 UI 전환
    /// </summary>
    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        switch (newState)
        {
            case ServerState.GameStart:
                Debug.Log("게임 시작");
                break;

            case ServerState.SessionStart:
                StartCoroutine(SessionStartRoutine());
                break;

            case ServerState.EscapeStart:
                // 현재 UI 페이드 아웃
                StartCoroutine(FadeMotion(false, curCg));
                break;

            case ServerState.SessionEnd:
                StartCoroutine(SessionEndRoutine());
                break;

            case ServerState.GameOver:
                StartCoroutine(GameOverRoutine());
                break;
        }
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    public void Init()
    {
        // UI 카메라 설정
        Camera uiCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = uiCamera;

        // 관전자 UI 초기화
        spectaterView.Init();
        
        // 초기 UI 상태 설정
        systemActivateCg.gameObject.SetActive(true);
        systemDownCg.gameObject.SetActive(false);
        checkOutCg.gameObject.SetActive(false);
        systemEnvCg.gameObject.SetActive(false);
        gameOverCg.gameObject.SetActive(false);
        
        gameObject.SetActive(true);

        // 시작 UI 지정
        curCg = systemActivateCg;
    }

    /// <summary>
    /// 씬 변경 시 UI 카메라 재설정
    /// </summary>
    public void GetUICamera(Scene scene, LoadSceneMode mode)
    {
        Camera uiCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = uiCamera;
    }

    /// <summary>
    /// 시스템 다운 UI 표시
    /// </summary>
    public void SystemDown()
    {
        StartCoroutine(SystemDownRoutine());
    }

    /// <summary>
    /// 시스템 활성 UI 표시
    /// </summary>
    public void SystemActivate()
    {
        StartCoroutine(SystemActivateRoutine());
    }

    // 시스템 활성 UI 페이드
    private IEnumerator SystemActivateRoutine()
    {
        yield return FadeRoutine(systemActivateCg);
    }

    // 시스템 다운 UI 페이드
    private IEnumerator SystemDownRoutine()
    {
        yield return FadeRoutine(systemDownCg);
    }

    // 세션 시작 UI 처리
    private IEnumerator SessionStartRoutine()
    {
        yield return FadeRoutine(systemActivateCg);
    }
    
    // 세션 종료 UI 처리 (정산 → 복귀)
    private IEnumerator SessionEndRoutine()
    {
        yield return FadeRoutine(checkOutCg);
        yield return new WaitForSeconds(1f);
        yield return FadeRoutine(systemActivateCg);
    }

    // 게임 종료 UI 처리
    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(2f);
        yield return FadeRoutine(gameOverCg);
    }

    // CanvasGroup 전환 로직
    private IEnumerator FadeRoutine(CanvasGroup nextCg)
    {
        // 같은 UI면 단순 페이드 인
        if (curCg == nextCg)
        {
            nextCg.gameObject.SetActive(true);
            yield return FadeMotion(true, nextCg);
            curCg = nextCg;
        }
        // 체크아웃 UI는 예외 처리
        else if (curCg != gameOverCg && nextCg == checkOutCg)
        {
            nextCg.gameObject.SetActive(true);
            yield return FadeMotion(true, nextCg);
            curCg = nextCg;
        }
        else
        {
            // 현재 UI 페이드 아웃
            yield return FadeMotion(false, curCg);

            // 비활성화
            curCg.gameObject.SetActive(false);

            yield return new WaitForSeconds(fadeDuration);

            // 다음 UI 활성화 + 페이드 인
            nextCg.gameObject.SetActive(true);
            yield return FadeMotion(true, nextCg);

            curCg = nextCg;
        }
    }
    
    /// <summary>
    /// CanvasGroup 알파 페이드 처리
    /// </summary>
    /// <param name="isFadeIn">true = 페이드 인, false = 페이드 아웃</param>
    /// <param name="cg">대상 CanvasGroup</param>
    public IEnumerator FadeMotion(bool isFadeIn, CanvasGroup cg)
    {
        float start = isFadeIn ? 0 : 1;
        float end = 1 - start;

        float time = 0;

        while (time < fadeDuration)
        {
            // 알파 값 보간
            float alpha = Mathf.Lerp(start, end, time / fadeDuration);
            cg.alpha = alpha;

            time += Time.deltaTime;
            yield return null;
        }

        // 최종값 보정
        cg.alpha = end;
    }
}
