using Unity.Collections;
using UnityEngine;

/// <summary>
/// 접속 상태 리스닝 인터페이스
/// </summary>
public interface IConnectionListener
{
    public void OnConnctionChanged(FixedString64Bytes username, bool isConnected);
}
