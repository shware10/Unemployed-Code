using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 정산결과 UI 뷰 클래스
/// </summary>
public class CheckOutView : MonoBehaviour
{
    // 플레이어 이름 텍스트
    [SerializeField] private TextMeshProUGUI userText;

    // 생존 아이콘
    [SerializeField] private Image aliveIcon;

    // 사망 아이콘
    [SerializeField] private Image deadIcon;

    // 노트 부모
    [SerializeField] private Transform noteParent;

    // 노트 프리팹
    [SerializeField] private GameObject note;

    /// <summary>
    /// UI 초기화
    /// </summary>
    public void Init()
    {
        // 기존 노트 제거
        foreach (Transform child in noteParent)
            Destroy(child.gameObject);

        // 초기 상태 리셋
        userText.SetText("");
        aliveIcon.enabled = false;
        deadIcon.enabled = false;
    }

    /// <summary>
    /// 플레이어 정보 표시
    /// </summary>
    /// <param name="userName">플레이어 이름</param>
    /// <param name="isAlive">생존 여부</param>
    public void ViewData(string userName, bool isAlive)
    {
        // 이름 표시
        userText.SetText(userName);

        // 상태 아이콘 표시
        aliveIcon.gameObject.SetActive(isAlive);
        deadIcon.gameObject.SetActive(!isAlive);
    }

    /// <summary>
    /// 노트(액션/점수량 기반) 추가
    /// </summary>
    /// <param name="msg">출력할 텍스트</param>
    public void ViewData(string msg)
    {
        // 노트 UI 생성
        GameObject noteObj = Instantiate(note, noteParent);

        // 텍스트 설정
        TextMeshProUGUI noteText = noteObj.GetComponent<TextMeshProUGUI>();
        noteText.SetText(msg);
    }
}

