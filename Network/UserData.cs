using System;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// 서버와 클라이언트 간에 동기화되는 플레이어 데이터 구조체
/// </summary>
public struct UserData : INetworkSerializable, IEquatable<UserData>
{
    // 로비/게임 UI에 표시되는 닉네임
    public FixedString64Bytes username;

    // 이 데이터를 소유한 Netcode 클라이언트 ID
    public ulong clientId;

    // 플레이어 행동량
    public int action;

    // 플레이어 점수
    public int score;

    // 생존 여부
    public bool isAlive;

    public UserData(FixedString64Bytes username, ulong clientId, int action = 0, int score = 0, bool isAlive = true)
    {
        this.clientId = clientId;
        this.username = username;
        this.action = action;
        this.score = score;
        this.isAlive = isAlive;
    }

    // 읽기/쓰기 경로 호환을 위해 고정된 순서로 직렬화
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
    
    // Dictionary, HashSet 등에서 사용됨
    public override int GetHashCode()
    {
        return HashCode.Combine(username, clientId, action, score, isAlive);
    }
}
