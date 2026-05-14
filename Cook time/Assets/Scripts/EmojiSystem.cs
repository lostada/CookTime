using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class EmojiSystem : NetworkBehaviour
{
    [Header("Prefabs dos Emojis")]
    public NetworkObject[] emojiPrefabs;

    [Header("Configuracao")]
    public Transform emojiSpawnPoint; // arrasta o Empty da cabeca aqui
    public float emojiDuration = 3f;

    private NetworkObject _currentEmoji;

    private Key[] emojiKeys = new Key[]
    {
        Key.F1, Key.F2, Key.F3, Key.F4,
        Key.F5, Key.F6, Key.F7, Key.F8
    };

    private void Update()
    {
        if (Object == null || !Object.HasInputAuthority) return;

        for (int i = 0; i < emojiKeys.Length; i++)
        {
            if (Keyboard.current[emojiKeys[i]].wasPressedThisFrame)
            {
                RPC_ShowEmoji(i);
                break;
            }
        }

        // Emoji sempre olha pra mesma direção do player
        if (_currentEmoji != null)
            _currentEmoji.transform.rotation = emojiSpawnPoint.rotation;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ShowEmoji(int emojiIndex)
    {
        if (emojiIndex < 0 || emojiIndex >= emojiPrefabs.Length) return;
        if (emojiPrefabs[emojiIndex] == null) return;

        if (_currentEmoji != null)
            Runner.Despawn(_currentEmoji);

        _currentEmoji = Runner.Spawn(
            emojiPrefabs[emojiIndex],
            emojiSpawnPoint.position,
            emojiSpawnPoint.rotation
        );

        _currentEmoji.transform.SetParent(emojiSpawnPoint);
        _currentEmoji.transform.localPosition = Vector3.zero;
        _currentEmoji.transform.localRotation = Quaternion.identity;

        StartCoroutine(RemoveEmoji());
    }

    private IEnumerator RemoveEmoji()
    {
        yield return new WaitForSeconds(emojiDuration);
        if (_currentEmoji != null)
        {
            Runner.Despawn(_currentEmoji);
            _currentEmoji = null;
        }
    }
}