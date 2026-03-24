using UnityEngine;

/// <summary>
/// 타이머 리스닝 인터페이스
/// </summary>
public interface ITimerListener
{
    public void OnTimerChanged(int time);
}
