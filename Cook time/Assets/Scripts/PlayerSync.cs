using Fusion;
using UnityEngine;

public class PlayerSync : NetworkBehaviour
{
    [Networked] private Vector3 NetworkedPosition { get; set; }
    [Networked] private Quaternion NetworkedRotation { get; set; }

    private void Update()
    {
        if (Object == null || !Object.HasInputAuthority) return;

        // Envia posicao pro servidor a cada frame
        RPC_UpdatePosition(transform.position, transform.rotation);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_UpdatePosition(Vector3 pos, Quaternion rot)
    {
        NetworkedPosition = pos;
        NetworkedRotation = rot;
    }

    public override void Render()
    {
        if (Object.HasInputAuthority) return;

        transform.position = Vector3.Lerp(transform.position, NetworkedPosition, Time.deltaTime * 15f);
        transform.rotation = Quaternion.Lerp(transform.rotation, NetworkedRotation, Time.deltaTime * 15f);
    }
}