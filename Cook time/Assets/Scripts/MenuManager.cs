using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Configuração")]
    [SerializeField] private int gameSceneIndex = 1;

    private NetworkRunner _runner;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        hostButton?.onClick.AddListener(() => StartGame(GameMode.Host));
        joinButton?.onClick.AddListener(() => StartGame(GameMode.AutoHostOrClient));
        quitButton?.onClick.AddListener(() => Application.Quit());
    }

    private async void StartGame(GameMode mode)
    {
        hostButton.interactable = false;
        joinButton.interactable = false;
        SetStatus("Conectando...");

        var go = new GameObject("NetworkRunner");
        DontDestroyOnLoad(go);
        _runner = go.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = "SalaDoSanduiche",
            Scene = SceneRef.FromIndex(gameSceneIndex),
            SceneManager = go.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
            SetStatus("Conectado! Carregando...");
        else
        {
            SetStatus($"Erro: {result.ShutdownReason}");
            hostButton.interactable = true;
            joinButton.interactable = true;
        }
    }

    // ✅ Quando a cena do jogo carrega, passa o Runner pro NetworkManager
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        NetworkManager nm = FindObjectOfType<NetworkManager>();
        if (nm != null)
            nm.SetRunner(runner);
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log(msg);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
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
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
}