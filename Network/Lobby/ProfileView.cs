using TMPro;
using UnityEngine;

/// <summary>
/// 현재 로그인된 유저 이름을 표시하는 프로필 UI
/// </summary>
public class ProfileView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userNameText;

    void Awake()
    {
        // AuthManager에서 현재 로그인 유저 이름 가져와 UI에 표시
        userNameText.SetText(AuthManager.Instance.userName);
    }
}
