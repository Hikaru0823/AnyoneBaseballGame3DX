using Fusion;
using UnityEngine;

public struct NetInput : INetworkInput
{
    public const byte READY = 1;
    public NetworkButtons buttons;
}
