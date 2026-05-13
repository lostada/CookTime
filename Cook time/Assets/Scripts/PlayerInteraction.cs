using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : NetworkBehaviour
{
    public float interactionRange = 3f;
    private PlayerRole myRole;
    private Camera myCamera;

    public override void Spawned()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (!Object.HasInputAuthority)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.gameObject.SetActive(false);
            return;
        }

        myRole = GetComponent<PlayerRole>();
        if (myRole == null)
        {
            Debug.LogError("PlayerRole não encontrado!");
            return;
        }

        myCamera = GetComponentInChildren<Camera>();
        if (myCamera == null)
            Debug.LogError("Nenhuma câmera encontrada no player!");
        else
            Debug.Log($"PlayerInteraction pronto! Role: {myRole.MyRole} | Camera: {myCamera.name}");
    }

    private void Update()
    {
        if (Object == null || !Object.HasInputAuthority) return;
        if (myCamera == null || myRole == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = myCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
            {
                Ingredient ingredient = hit.collider.GetComponent<Ingredient>();
                if (ingredient == null) return;

                string tag = hit.collider.tag;
                float dist = Vector3.Distance(transform.position, hit.point);

                Debug.Log($"[CLICK] {myRole.MyRole} clicou em {hit.collider.name} | tag: {tag} | dist: {dist:F2}m");

                if (!myRole.CanPickupByTag(tag))
                {
                    Debug.Log($"❌ {myRole.MyRole} não pode pegar {hit.collider.name}");
                    return;
                }

                if (GameManager.Instance == null) return;

                bool isCorrect = GameManager.Instance.IsCorrectIngredient(hit.collider.name, myRole.MyRole);
                if (!isCorrect)
                {
                    Debug.Log($"❌ Ingrediente errado! Pedido pede: {GameManager.Instance.GetCurrentOrder(myRole.MyRole)}");
                    return;
                }

                GameManager.Instance.TryAddIngredient(hit.collider.name, Runner.LocalPlayer, myRole.MyRole);
            }
        }
    }
}