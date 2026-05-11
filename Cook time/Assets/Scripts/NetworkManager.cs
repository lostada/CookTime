using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TextMeshProUGUI statusText;

    private NetworkRunner runner;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (hostButton != null)
            hostButton.onClick.AddListener(() => StartGame(GameMode.Host));

        if (joinButton != null)
            joinButton.onClick.AddListener(() => StartGame(GameMode.Client));
    }

    private async void StartGame(GameMode mode)
    {
        if (statusText != null)
            statusText.text = "Conectando...";

        runner = gameObject.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        string sessionName = string.IsNullOrEmpty(sessionNameInput.text) ? "CookTimeRoom" : sessionNameInput.text;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            PlayerCount = 3,
            IsVisible = true,
            IsOpen = true
        };

        var result = await runner.StartGame(startGameArgs);

        if (result.Ok)
        {
            Debug.Log($"Conectado! Modo: {mode}");
            if (statusText != null)
                statusText.text = $"Conectado! Sala: {sessionName}";

            if (hostButton != null) hostButton.gameObject.SetActive(false);
            if (joinButton != null) joinButton.gameObject.SetActive(false);

            await Task.Delay(100);
            SpawnPlayerWithRole();
        }
        else
        {
            Debug.LogError($"Erro ao conectar: {result.ShutdownReason}");
            if (statusText != null)
                statusText.text = $"Erro: {result.ShutdownReason}";
        }
    }

    private void SpawnPlayerWithRole()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            if (gameManager == null) return;
        }

        int connectedPlayers = runner.ActivePlayers.Count();
        RoleType myRole = (RoleType)((connectedPlayers - 1) % 3);
        if (myRole < 0) myRole = RoleType.BreadMaster;

        NetworkObject mySkin = gameManager.GetSkinForRole(myRole);
        if (mySkin == null) return;

        Transform myTable = gameManager.GetTableForRole(myRole);
        Vector3 spawnPos = myTable != null ? myTable.position + new Vector3(0, 1, -1.5f) : new Vector3(0, 1, 0);

        var playerObj = runner.Spawn(mySkin, spawnPos, Quaternion.identity, runner.LocalPlayer);

        PlayerRole roleScript = playerObj.GetComponent<PlayerRole>();
        if (roleScript != null)
            roleScript.MyRole = myRole;

        gameManager.PlayerConnected();

        Debug.Log($"🎮 Jogador spawnado como {myRole}");
    }
}