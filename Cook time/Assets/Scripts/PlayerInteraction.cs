using Fusion;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Configurações")]
    public float interactionRange = 3f;

    private PlayerRole myRole;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            myRole = GetComponent<PlayerRole>();
            if (myRole == null)
                Debug.LogError("PlayerRole não encontrado!");
            else
                Debug.Log($"PlayerInteraction pronto! Role: {myRole.MyRole}");
        }
    }

    public void TryPickup(GameObject ingredient)
    {
        if (!Object.HasInputAuthority) return;

        float dist = Vector3.Distance(transform.position, ingredient.transform.position);
        Debug.Log($"[CLICK] {myRole.MyRole} clicou em {ingredient.name} | Distância: {dist:F2}m");

        if (dist > interactionRange)
        {
            Debug.LogWarning($"Muito longe! ({dist:F2}m) Chega mais perto.");
            return;
        }

        string tag = ingredient.tag;
        if (!myRole.CanPickupByTag(tag))
        {
            Debug.Log($"❌ {myRole.MyRole} não pode pegar {ingredient.name} (tag: {tag})");
            return;
        }

        Debug.Log($"✅ {myRole.MyRole} pegou: {ingredient.name}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TryAddIngredient(ingredient.name, Runner.LocalPlayer, myRole.MyRole);
            GameManager.Instance.RemoveIngredient(ingredient, myRole.MyRole);
        }

        NetworkObject netObj = ingredient.GetComponent<NetworkObject>();
        if (netObj != null)
            Runner.Despawn(netObj);
        else
            Destroy(ingredient);
    }
}