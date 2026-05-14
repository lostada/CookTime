using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum RoleType { BreadMaster, MeatMaster, CheeseMaster }

public class PlayerRole : NetworkBehaviour
{
    [Networked] public RoleType MyRole { get; set; }

    [Header("UI")]
    public TextMeshProUGUI roleText;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority) return;

        // Busca o texto automaticamente se nao estiver preenchido
        if (roleText == null)
            roleText = GameObject.Find("RoleText")?.GetComponent<TextMeshProUGUI>();

        int idx = (Runner.LocalPlayer.PlayerId - 1) % 3;
        RPC_SetRole(idx);
        Debug.Log($"Pedindo role: {(RoleType)idx}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetRole(int idx)
    {
        MyRole = (RoleType)idx;
        Debug.Log($"Role setada: {MyRole}");
    }

    public override void Render()
    {
        if (!Object.HasInputAuthority) return;
        if (roleText == null) return;

        string roleName = MyRole switch
        {
            RoleType.BreadMaster => "Mestre do Pao",
            RoleType.MeatMaster => "Mestre da Carne",
            RoleType.CheeseMaster => "Mestre do Queijo",
            _ => "?"
        };

        roleText.text = $"Sua funcao: {roleName}";
    }

    private void Update()
    {
        if (Object == null || !Object.HasInputAuthority) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) RPC_SetRole(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) RPC_SetRole(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) RPC_SetRole(2);
    }

    public bool CanPickupByTag(string tag)
    {
        return (MyRole == RoleType.BreadMaster && tag == "Bread") ||
               (MyRole == RoleType.MeatMaster && tag == "Meat") ||
               (MyRole == RoleType.CheeseMaster && tag == "Cheese");
    }
}