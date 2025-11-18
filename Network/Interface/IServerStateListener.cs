using UnityEngine;

public interface IServerStateListener
{
    public void OnStateChanged(ServerState oldState, ServerState newState);
}
