using Fusion;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerSetup : NetworkBehaviour
{
    private FirstPersonController fpc;
    private PlayerInput playerInput;
    private CharacterController cc;

    private void Awake()
    {
        fpc = GetComponent<FirstPersonController>();
        playerInput = GetComponent<PlayerInput>();
        cc = GetComponent<CharacterController>();

        if (fpc != null) fpc.enabled = false;
        if (playerInput != null) playerInput.enabled = false;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
            StartCoroutine(ActivateControls());
        else
        {
            if (fpc != null) fpc.enabled = false;
            if (playerInput != null) playerInput.enabled = false;
            GetComponentInChildren<Camera>()?.gameObject.SetActive(false);
            GetComponentInChildren<AudioListener>()?.gameObject.SetActive(false);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;
        if (fpc == null || !fpc.enabled) return;
        if (!GetInput(out NetworkInputData input)) return;

        fpc.SetInput(input.moveDirection, input.lookDelta);
    }

    private IEnumerator ActivateControls()
    {
        yield return null;
        yield return null;
        yield return null;

        if (cc != null) cc.enabled = false;
        yield return null;
        if (cc != null) cc.enabled = true;

        if (playerInput != null) playerInput.enabled = true;
        yield return null;
        if (fpc != null) fpc.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;  // <-- muda aqui
        Cursor.visible = false;                      // <-- e aqui

        Debug.Log("Player local ativado!");
    }
}