using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 보이스 채팅 말하기 상태에 따라 아이콘 표시
/// </summary>
public class Chat : MonoBehaviour
{
    // 말하기 상태 아이콘
    [SerializeField] private Image audioImage;
    
    /// <summary>
    /// 말하기 상태에 따라 아이콘 활성/비활성
    /// </summary>
    /// <param name="isActive">true = 말하는 중, false = 비활성</param>
    public void ActiveImage(bool isActive)
    {
        audioImage.enabled = isActive;
    }
}
