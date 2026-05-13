using Fusion;
using UnityEngine;

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
        // ✅ mesma correção do PlayerInteraction
        if (Object == null || !Object.HasInputAuthority) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) { MyRole = RoleType.BreadMaster; Debug.Log("Role: BreadMaster"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { MyRole = RoleType.MeatMaster; Debug.Log("Role: MeatMaster"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { MyRole = RoleType.CheeseMaster; Debug.Log("Role: CheeseMaster"); }
    }

    public bool CanPickupByTag(string tag)
    {
        return (MyRole == RoleType.BreadMaster && tag == "Bread") ||
               (MyRole == RoleType.MeatMaster && tag == "Meat") ||
               (MyRole == RoleType.CheeseMaster && tag == "Cheese");
    }
}