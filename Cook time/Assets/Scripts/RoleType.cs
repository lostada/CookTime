using Fusion;
using UnityEngine;

public enum RoleType
{
    BreadMaster,
    MeatMaster,
    CheeseMaster
}

public class PlayerRole : NetworkBehaviour
{
    [Networked]
    public RoleType MyRole { get; set; }

    private GameManager gameManager;

    public override void Spawned()
    {
        gameManager = GameManager.Instance;

        if (Object.HasInputAuthority)
        {
            Debug.Log($"🎭 Meu papel é: {MyRole}");
        }
    }

    public bool CanPickupIngredient(string ingredientName)
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (gameManager == null) return false;

        switch (MyRole)
        {
            case RoleType.BreadMaster:
                return System.Array.Exists(gameManager.breadOptions, x => x == ingredientName);
            case RoleType.MeatMaster:
                return System.Array.Exists(gameManager.meatOptions, x => x == ingredientName);
            case RoleType.CheeseMaster:
                return System.Array.Exists(gameManager.cheeseOptions, x => x == ingredientName);
            default:
                return false;
        }
    }

    public Color GetRoleColor()
    {
        switch (MyRole)
        {
            case RoleType.BreadMaster: return new Color(0.8f, 0.5f, 0.2f);
            case RoleType.MeatMaster: return Color.red;
            case RoleType.CheeseMaster: return Color.yellow;
            default: return Color.white;
        }
    }
}