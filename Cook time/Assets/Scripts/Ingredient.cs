using Fusion;
using UnityEngine;

public class Ingredient : NetworkBehaviour
{
    private void OnMouseDown()
    {
        // Acha o player local
        PlayerInteraction interaction = FindLocalPlayer();
        if (interaction == null)
        {
            Debug.LogWarning("PlayerInteraction local não encontrado!");
            return;
        }

        interaction.TryPickup(this.gameObject);
    }

    private PlayerInteraction FindLocalPlayer()
    {
        foreach (PlayerInteraction p in FindObjectsOfType<PlayerInteraction>())
        {
            if (p.Object != null && p.Object.HasInputAuthority)
                return p;
        }
        return null;
    }
}