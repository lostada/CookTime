using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_InputField sessionNameInput;
    [SerializeField] private TextMeshProUGUI statusText;

    private NetworkRunner runner;

    private void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(() => StartGame(GameMode.Host));

        if (joinButton != null)
            joinButton.onClick.AddListener(() => StartGame(GameMode.Client));
    }

    private async void StartGame(GameMode mode)
    {
        if (statusText != null)
            statusText.text = "Conectando...";

        // Cria um GameObject separado para o NetworkRunner se não existir
        GameObject runnerObj = new GameObject("NetworkRunner");
        runner = runnerObj.AddComponent<NetworkRunner>();
        runner.ProvideInput = true;

        // Mantém o objeto entre cenas
        DontDestroyOnLoad(runnerObj);

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
            Debug.Log($"✅ Conectado! Modo: {mode}");
            if (statusText != null)
                statusText.text = $"Conectado! Sala: {sessionName}";

            if (hostButton != null) hostButton.gameObject.SetActive(false);
            if (joinButton != null) joinButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"❌ Erro ao conectar: {result.ShutdownReason}");
            if (statusText != null)
                statusText.text = $"Erro: {result.ShutdownReason}";
        }
    }

    public void Disconnect()
    {
        if (runner != null)
            runner.Shutdown();
    }
}