using UnityEngine;

/// <summary>
/// 발화 리스닝 인터페이스
/// </summary>
public interface ISpeechListener
{
    public void OnSpeechDetected(string username, bool isActive);
}
