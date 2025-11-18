using Unity.Collections;
using UnityEngine;

public interface IConnectionListener
{
    public void OnConnctionChanged(FixedString64Bytes username, bool isConnected);
}
