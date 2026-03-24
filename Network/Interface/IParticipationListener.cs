using System.Collections.Generic;
using Unity.Services.Vivox;


/// <summary>
/// 음성채팅 채널 참여 리스닝 인터페이스
/// </summary>
public interface IParticipationListener
{
    public void OnParticipantChanged(List<VivoxParticipant> participants);
}
