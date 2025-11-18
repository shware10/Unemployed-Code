using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour,IServerStateListener
{
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private SpectaterView spectaterView;
    
    public CanvasGroup systemActivateCg;
    public CanvasGroup systemDownCg;
    public CanvasGroup checkOutCg;
    public CanvasGroup systemEnvCg;
    public CanvasGroup gameOverCg;

    public CanvasGroup curCg;
    
    public void OnStateChanged(ServerState oldState, ServerState newState)
    {
        switch (newState)
        {
            case ServerState.GameStart :
                Debug.Log("게임 시작");
                break;
            case ServerState.SessionStart :
                StartCoroutine(SessionStartRoutine());
                break;
            case ServerState.EscapeStart :
                StartCoroutine(FadeMotion(false, curCg));
                break;
            case ServerState.SessionEnd :
                StartCoroutine(SessionEndRoutine());
                break;
            case ServerState.GameOver :
                StartCoroutine(GameOverRoutine());
                break;
        }
    }

    public void Init()
    {
        Camera uiCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = uiCamera;

        spectaterView.Init();
        
        systemActivateCg.gameObject.SetActive(true);
        systemDownCg.gameObject.SetActive(false);
        checkOutCg.gameObject.SetActive(false);
        systemEnvCg.gameObject.SetActive(false);
        gameOverCg.gameObject.SetActive(false);
        
        gameObject.SetActive(true);
        curCg = systemActivateCg;
    }

    public void GetUICamera(Scene scene, LoadSceneMode mode)
    {
        Camera uiCamera = GameObject.FindWithTag("UICamera").GetComponent<Camera>();
        GetComponent<Canvas>().worldCamera = uiCamera;
    }

    public void SystemDown()
    {
        StartCoroutine(SystemDownRoutine());
    }

    public void SystemActivate()
    {
        StartCoroutine(SystemActivateRoutine());
    }

    IEnumerator SystemActivateRoutine()
    {
        yield return FadeRoutine(systemActivateCg);
    }
    IEnumerator SystemDownRoutine()
    {
        yield return FadeRoutine(systemDownCg);
    }
    IEnumerator SessionStartRoutine()
    {
        yield return FadeRoutine(systemActivateCg);
    }
    
    IEnumerator SessionEndRoutine()
    {
        yield return FadeRoutine(checkOutCg);
        yield return new WaitForSeconds(1f);
        yield return FadeRoutine(systemActivateCg);
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(2f);
        yield return FadeRoutine(gameOverCg);
    }

    IEnumerator FadeRoutine(CanvasGroup nextCg)
    {
        if (curCg == nextCg)
        {
            nextCg.gameObject.SetActive(true);
            yield return FadeMotion(true, nextCg);
            curCg = nextCg;
        }
        else if (curCg != gameOverCg && nextCg == checkOutCg)
        {
            nextCg.gameObject.SetActive(true);
            yield return FadeMotion(true, nextCg);
            curCg = nextCg;
        }
        else
        {
            yield return FadeMotion(false, curCg);
            curCg.gameObject.SetActive(false);
            yield return new WaitForSeconds(fadeDuration);
            nextCg.gameObject.SetActive(true);
            yield return FadeMotion(true, nextCg);
            curCg = nextCg;
        }
    }
    
    public IEnumerator FadeMotion(bool isFadeIn, CanvasGroup cg)
    {
        float start = isFadeIn ? 0 : 1;
        float end = 1 - start;
        float time = 0;
        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(start, end, time / fadeDuration);
            cg.alpha = alpha;

            time += Time.deltaTime;
            yield return null;
        }

        cg.alpha = end;
    }
}

