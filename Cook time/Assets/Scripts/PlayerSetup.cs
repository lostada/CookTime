using Fusion;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSetup : NetworkBehaviour
{
    private FirstPersonController fpc;
    private PlayerInput playerInput;

    public override void Spawned()
    {
        fpc = GetComponent<FirstPersonController>();
        playerInput = GetComponent<PlayerInput>();

        if (Object.HasInputAuthority)
        {
            // Player local — ativa tudo
            fpc.enabled = true;
            Debug.Log("Player local — controles ativados!");
        }
        else
        {
            // Player remoto — desativa tudo
            fpc.enabled = false;
            if (playerInput != null) playerInput.enabled = false;
            GetComponentInChildren<Camera>()?.gameObject.SetActive(false);
            Debug.Log("Player remoto — controles desativados!");
        }
    }
}