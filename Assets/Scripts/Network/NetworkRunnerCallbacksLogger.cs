using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RunnerCallbacksLogger : MonoBehaviour, INetworkRunnerCallbacks
{
    public event Action<NetworkRunner, ShutdownReason> OnShutdownEvent;
    public event Action<NetworkRunner, NetDisconnectReason> OnDisconnectedEvent;

    void Awake()
    {
        var runner = GetComponent<NetworkRunner>();
        if (runner != null)
        {
            runner.AddCallbacks(this);
            Debug.Log($"[RunnerCallbacksLogger] AddCallbacks to Runner id={runner.GetInstanceID()}");
        }
        else
        {
            Debug.LogError("[RunnerCallbacksLogger] NetworkRunner not found!");
        }
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[RunnerCallbacksLogger] OnShutdown runner={runner.GetInstanceID()}, reason={shutdownReason}");
        OnShutdownEvent?.Invoke(runner, shutdownReason);
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[RunnerCallbacksLogger] OnDisconnectedFromServer runner={runner.GetInstanceID()}, reason={reason}");
        OnDisconnectedEvent?.Invoke(runner, reason);
    }

    // 他のコールバックは空実装
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }
}