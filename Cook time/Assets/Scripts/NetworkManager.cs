using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Players (ordem de entrada)")]
    [SerializeField] private NetworkObject prefabNeckel;
    [SerializeField] private NetworkObject prefabLostada;
    [SerializeField] private NetworkObject prefabVentura;

    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform spawnPoint3;

    private int _playerCount = 0;
    private NetworkRunner _runner;

    public void SetRunner(NetworkRunner runner)
    {
        _runner = runner;
        _runner.AddCallbacks(this);
        Debug.Log("NetworkManager registrado no runner via SetRunner!");
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        _playerCount++;
        NetworkObject prefab = _playerCount switch
        {
            1 => prefabNeckel,
            2 => prefabLostada,
            3 => prefabVentura,
            _ => prefabNeckel
        };
        Transform spawnPoint = _playerCount switch
        {
            1 => spawnPoint1,
            2 => spawnPoint2,
            3 => spawnPoint3,
            _ => spawnPoint1
        };

        if (prefab == null) { Debug.LogError($"Prefab {_playerCount} nao preenchido!"); return; }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        runner.Spawn(prefab, pos, rot, player);
        Debug.Log($"Jogador {_playerCount} spawnado em {pos} como {prefab.name}");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        SpawnPlayer(runner, player);
    }

    // ✅ CORRIGIDO: coleta o input a cada tick do Fusion
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (Keyboard.current != null)
        {
            float h = 0f, v = 0f;
            if (Keyboard.current.wKey.isPressed) v = 1f;
            if (Keyboard.current.sKey.isPressed) v = -1f;
            if (Keyboard.current.aKey.isPressed) h = -1f;
            if (Keyboard.current.dKey.isPressed) h = 1f;
            data.moveDirection = new Vector2(h, v);
        }

        if (Mouse.current != null)
            data.lookDelta = Mouse.current.delta.ReadValue();

        data.buttons.Set(PlayerButton.Jump, Keyboard.current != null && Keyboard.current.spaceKey.isPressed);

        input.Set(data);
    }

    public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { _playerCount--; }
    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason reason) { }
    public void OnConnectedToServer(NetworkRunner r) { }
    public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] token) { }
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> list) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
}