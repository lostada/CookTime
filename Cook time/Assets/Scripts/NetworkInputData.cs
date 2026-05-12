using Fusion;
using UnityEngine;

public enum PlayerButton
{
    Jump = 0,
    Fire = 1,
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 moveDirection;
    public NetworkButtons buttons;
}