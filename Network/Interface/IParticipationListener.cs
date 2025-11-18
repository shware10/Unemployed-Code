using System.Collections.Generic;
using Unity.Services.Vivox;


public interface IParticipationListener
{
    public void OnParticipantChanged(List<VivoxParticipant> participants);
}
