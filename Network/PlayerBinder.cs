using System.Collections;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBinder : NetworkBehaviour
{
    [SerializeField] private UIManager uiManager;
    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            StartCoroutine(DelaySendData());
        }
    }
    
    IEnumerator DelaySendData()
    {
        yield return null;
        
        GameServer.Instance.BindServer(gameObject);
        GameServer.Instance.SendDataServerRpc(AuthManager.Instance.userName);
        uiManager.Init();
        SceneManager.sceneLoaded += uiManager.GetUICamera;
    }
}
