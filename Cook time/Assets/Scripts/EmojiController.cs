using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmojiController : NetworkBehaviour
{
    [SerializeField] private Button emojiButton;
    [SerializeField] private Transform emojiSpawnPoint;

    private string[] emojis = { "😀", "🍞", "🧀", "🌶️", "✅", "❌", "🤔", "🏃", "🎉", "👀" };

    public override void Spawned()
    {
        if (Object.HasInputAuthority && emojiButton != null)
        {
            emojiButton.onClick.AddListener(SendRandomEmoji);
        }
    }

    private void SendRandomEmoji()
    {
        string randomEmoji = emojis[Random.Range(0, emojis.Length)];
        RPC_ShowEmoji(randomEmoji);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_ShowEmoji(string emoji)
    {
        Vector3 spawnPos = emojiSpawnPoint != null ? emojiSpawnPoint.position : transform.position + Vector3.up * 1.8f;

        GameObject textObj = new GameObject("Emoji");
        textObj.transform.position = spawnPos;

        var tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = emoji;
        tmp.fontSize = 5;
        tmp.alignment = TextAlignmentOptions.Center;

        textObj.transform.localScale = Vector3.one * 0.3f;

        Destroy(textObj, 2f);
    }
}