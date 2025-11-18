using TMPro;
using UnityEngine;

public class ProfileView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userNameText;

    void Awake()
    {
        userNameText.SetText(AuthManager.Instance.userName);
    }
}
