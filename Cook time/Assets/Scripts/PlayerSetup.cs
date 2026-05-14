using Fusion;
using StarterAssets;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    private FirstPersonController _fpc;

    public override void Spawned()
    {
        _fpc = GetComponent<FirstPersonController>();

        if (Object.HasInputAuthority)
        {
            // ✅ Sem coroutine — ativa direto e de forma confiável
            _fpc.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Player local — controles ativados!");
        }
        else
        {
            _fpc.enabled = false;
            GetComponentInChildren<Camera>()?.gameObject.SetActive(false);
            Debug.Log("Player remoto — controles desativados!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        // ✅ Pega o input que o Fusion coletou no OnInput e passa pro FPC
        if (!Object.HasInputAuthority) return;
        if (!GetInput(out NetworkInputData input)) return;

        _fpc.SetInput(input.moveDirection, input.lookDelta);
    }
}