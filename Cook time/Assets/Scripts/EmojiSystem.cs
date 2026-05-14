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

    [Networked] private NetworkId currentEmojiId { get; set; }
    private NetworkObject _currentEmoji;

    private Key[] emojiKeys = new Key[]
    {
        Key.F1, Key.F2, Key.F3, Key.F4,
        Key.F5, Key.F6, Key.F7, Key.F8, Key.F9
    };

    private void Update()
    {
        // Só o dono do personagem pode enviar emojis
        if (Object == null || !Object.HasInputAuthority) return;

        for (int i = 0; i < emojiKeys.Length; i++)
        {
            if (Keyboard.current[emojiKeys[i]].wasPressedThisFrame)
            {
                RPC_ShowEmoji(i);
                break;
            }
        }

        // Sincroniza a rotação do emoji com o spawn point em todos os clients
        if (_currentEmoji != null && emojiSpawnPoint != null)
            _currentEmoji.transform.rotation = emojiSpawnPoint.rotation;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ShowEmoji(int emojiIndex)
    {
        if (emojiIndex < 0 || emojiIndex >= emojiPrefabs.Length) return;
        if (emojiPrefabs[emojiIndex] == null) return;

        // Remove emoji atual se existir
        if (_currentEmoji != null)
        {
            Runner.Despawn(_currentEmoji);
            _currentEmoji = null;
        }

        // Faz o spawn do emoji na posição do spawn point
        var emoji = Runner.Spawn(
            emojiPrefabs[emojiIndex],
            emojiSpawnPoint.position,
            emojiSpawnPoint.rotation,
            Object.InputAuthority  // ← Importante: define quem é o dono
        );

        // Guarda o NetworkId para sincronizar entre clients
        currentEmojiId = emoji.Id;

        // Chama o RPC para todos os clients mostrarem o emoji
        RPC_OnEmojiSpawned(emoji.Id, emojiIndex);

        // Inicia a coroutine para remover depois
        StartCoroutine(RemoveEmojiAfterDelay(emoji.Id));
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OnEmojiSpawned(NetworkId emojiId, int emojiIndex)
    {
        // Procura o emoji que foi spawnado pelo ID
        if (Runner.TryFindObject(emojiId, out NetworkObject emojiObj))
        {
            // Se for o dono, já tem referência
            if (Object.HasInputAuthority)
            {
                _currentEmoji = emojiObj;
            }

            // Parenteia o emoji no spawn point do jogador CORRETO
            // Precisa encontrar o spawn point do jogador que mandou o emoji
            if (emojiSpawnPoint != null)
            {
                emojiObj.transform.SetParent(emojiSpawnPoint);
                emojiObj.transform.localPosition = Vector3.zero;
                emojiObj.transform.localRotation = Quaternion.identity;
            }
        }
    }

    private IEnumerator RemoveEmojiAfterDelay(NetworkId emojiId)
    {
        yield return new WaitForSeconds(emojiDuration);

        // Remove o emoji de todos os clients
        RPC_RemoveEmoji(emojiId);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RemoveEmoji(NetworkId emojiId)
    {
        if (Runner.TryFindObject(emojiId, out NetworkObject emojiObj))
        {
            Runner.Despawn(emojiObj);

            if (Object.HasInputAuthority)
                _currentEmoji = null;
        }
    }
}