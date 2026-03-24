using UnityEngine;

/// <summary>
/// 서버 상태 리스닝 인터페이스
/// </summary>
public interface IServerStateListener
{
    public void OnStateChanged(ServerState oldState, ServerState newState);
}
