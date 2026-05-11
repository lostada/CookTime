using Fusion;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Raycast")]
    public float interactionRange = 2.5f;
    public LayerMask ingredientLayer;

    private Camera playerCamera;
    private GameManager gameManager;
    private PlayerRole playerRole;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        gameManager = GameManager.Instance;
        playerRole = GetComponent<PlayerRole>();
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;
        if (playerCamera == null) return;
        if (gameManager == null || playerRole == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, ingredientLayer))
        {
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();
            if (ingredient != null && Input.GetMouseButtonDown(0))
            {
                // Verifica se está na mesa certa
                if (gameManager.IsPlayerAtCorrectTable(transform, playerRole.MyRole))
                {
                    gameManager.TryAddIngredient(
                        ingredient.ingredientName,
                        Runner.LocalPlayer,
                        playerRole.MyRole
                    );
                }
                else
                {
                    Debug.Log("Você precisa estar na SUA mesa!");
                }
            }
        }
    }
}