using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputProvider : MonoBehaviour, INetworkRunnerCallbacks
{
    private Vector2 _moveInput;
    private Vector2 _lookInput;

    private void Update()
    {
        // Pega o input do New Input System
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null)
        {
            _moveInput = Vector2.zero;
            if (kb.wKey.isPressed) _moveInput.y += 1;
            if (kb.sKey.isPressed) _moveInput.y -= 1;
            if (kb.aKey.isPressed) _moveInput.x -= 1;
            if (kb.dKey.isPressed) _moveInput.x += 1;
            _moveInput = _moveInput.normalized;
        }

        if (mouse != null)
            _lookInput = mouse.delta.ReadValue();
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData
        {
            moveDirection = _moveInput,
            lookDelta = _lookInput,
        };

        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            data.buttons.Set(PlayerButton.Jump, true);

        input.Set(data);
        _lookInput = Vector2.zero; // reseta pra não acumular
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}