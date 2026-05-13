using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class NetworkLauncher : MonoBehaviour
{
    public NetworkRunner runnerPrefab; // arraste o prefab do Runner aqui
    private NetworkRunner runner;

    [Header("UI opcional")]
    public Button hostButton;
    public Button joinButton;

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(() => StartGame(GameMode.Host));
        if (joinButton != null) joinButton.onClick.AddListener(() => StartGame(GameMode.Client));
    }

    public async void StartGame(GameMode mode)
    {
        runner = Instantiate(runnerPrefab);
        runner.AddCallbacks(GetComponent<InputProvider>()); // se tiver InputProvider aqui

        await runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = "SalaDoSanduiche",
            Scene = SceneRef.FromIndex(0), // índice da sua cena no Build Settings
        });

        Debug.Log($"Conectado como {mode}!");
    }
}