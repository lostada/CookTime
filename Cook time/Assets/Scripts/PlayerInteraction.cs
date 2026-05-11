using Fusion;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Configurações")]
    public float interactionRange = 2.5f;
    public LayerMask ingredientLayer;

    private Camera playerCamera;
    private GameManager gameManager;
    private PlayerRole playerRole;
    private bool isInitialized = false;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            // Encontra os componentes
            FindComponents();
            isInitialized = true;
        }
    }

    private void FindComponents()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        playerRole = GetComponent<PlayerRole>();

        Debug.Log($"[PlayerInteraction] Inicializado - Camera: {playerCamera != null}, GameManager: {gameManager != null}, PlayerRole: {playerRole != null}");
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;
        if (!isInitialized) return;

        // Verificações de segurança
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) return;
        }

        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            if (gameManager == null) return;
        }

        if (playerRole == null)
        {
            playerRole = GetComponent<PlayerRole>();
            if (playerRole == null) return;
        }

        // Raycast
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, ingredientLayer))
        {
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();

            if (ingredient != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log($"[PlayerInteraction] Pegou: {ingredient.ingredientName}");
                    gameManager.TryAddIngredient(
                        ingredient.ingredientName,
                        Runner.LocalPlayer,
                        playerRole.MyRole
                    );
                }
            }
        }
    }
}