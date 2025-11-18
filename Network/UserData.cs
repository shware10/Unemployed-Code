using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// 서버에서 관리할 유저데이터
/// </summary>
public struct UserData : INetworkSerializable, IEquatable<UserData>
{
    public FixedString64Bytes username;
    public ulong clientId;
    public int action;
    public int score;
    public bool isAlive; 

    public UserData(FixedString64Bytes username, ulong clientId, int action = 0, int score = 0, bool isAlive = true)
    {
        this.clientId = clientId;
        this.username = username;
        this.action = action;
        this.score = score;
        this.isAlive = isAlive;
    }

    // Netcode 직렬화/역직렬화 구현
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref action);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref isAlive);
    }
    public bool Equals(UserData other)
    {
        return username.Equals(other.username)
               && clientId == other.clientId
               && action == other.action
               && score == other.score
               && isAlive == other.isAlive;
    }

    public override bool Equals(object obj)
    {
        return obj is UserData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(username, clientId, action, score, isAlive);
    }
}