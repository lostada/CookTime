using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField roomInput;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Player")]
    [SerializeField] private NetworkObject playerPrefab;

    private NetworkRunner _runner;

    private void Start()
    {
        hostButton?.onClick.AddListener(() => StartGame(GameMode.Host));
        joinButton?.onClick.AddListener(() => StartGame(GameMode.Client));
    }

    private async void StartGame(GameMode mode)
    {
        SetStatus("Conectando...");

        var go = new GameObject("NetworkRunner");
        DontDestroyOnLoad(go);

        _runner = go.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        string room = string.IsNullOrEmpty(roomInput?.text) ? "Sala1" : roomInput.text;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = room,
            Scene = SceneRef.FromIndex(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = go.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            SetStatus($"Conectado! Sala: {room}");
            hostButton?.gameObject.SetActive(false);
            joinButton?.gameObject.SetActive(false);
        }
        else
        {
            SetStatus($"Erro: {result.ShutdownReason}");
        }
    }

    // Spawna o player quando alguém entra
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 pos = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
            runner.Spawn(playerPrefab, pos, Quaternion.identity, player);
        }
    }

    // Envia input pro Fusion
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // PlayerController lê Input direto no FixedUpdateNetwork
        // esse callback precisa existir mas pode ficar vazio
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }

    public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { }
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