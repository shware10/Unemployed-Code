using UnityEngine;

public interface ISpeechListener
{
    public void OnSpeechDetected(string username, bool isActive);
}
