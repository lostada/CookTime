using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public enum RoleType { BreadMaster, MeatMaster, CheeseMaster }

public class PlayerRole : NetworkBehaviour
{
    [Networked] public RoleType MyRole { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            int idx = (Runner.LocalPlayer.PlayerId - 1) % 3;
            MyRole = (RoleType)idx;
            Debug.Log($"Meu papel: {MyRole}");
        }
    }

    private void Update()
    {
        if (Object == null || !Object.HasInputAuthority) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) { MyRole = RoleType.BreadMaster; Debug.Log("Role: BreadMaster"); }
        if (Keyboard.current.digit2Key.wasPressedThisFrame) { MyRole = RoleType.MeatMaster; Debug.Log("Role: MeatMaster"); }
        if (Keyboard.current.digit3Key.wasPressedThisFrame) { MyRole = RoleType.CheeseMaster; Debug.Log("Role: CheeseMaster"); }
    }

    public bool CanPickupByTag(string tag)
    {
        return (MyRole == RoleType.BreadMaster && tag == "Bread") ||
               (MyRole == RoleType.MeatMaster && tag == "Meat") ||
               (MyRole == RoleType.CheeseMaster && tag == "Cheese");
    }
}