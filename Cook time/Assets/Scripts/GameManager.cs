using Fusion;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Opções de cada jogador")]
    public string[] breadOptions = { "PaoAustraliano", "PaoBrioche" };
    public string[] meatOptions = { "CalabresaAcebolada", "Frango", "CarneMoida" };
    public string[] cheeseOptions = { "QueijoPrato", "Cheddar" };

    [Header("Ingredientes na Cena")]
    public GameObject[] breadObjects;
    public GameObject[] meatObjects;
    public GameObject[] cheeseObjects;

    [Header("UI")]
    public TextMeshProUGUI orderText;

    [Networked] public NetworkString<_32> currentOrderBread { get; set; }
    [Networked] public NetworkString<_32> currentOrderMeat { get; set; }
    [Networked] public NetworkString<_32> currentOrderCheese { get; set; }
    [Networked] public int currentPlateStage { get; set; }

    private int completedOrders = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (Runner == null)
        {
            Debug.Log("Modo offline - gerando pedido local");
            GenerateNewOrderLocal();
        }
    }

    public override void Spawned()
    {
        Debug.Log($"GameManager Spawned! HasStateAuthority: {Object.HasStateAuthority}");
        if (Object.HasStateAuthority)
        {
            GenerateNewOrder();
            Debug.Log("GenerateNewOrder chamado!");
        }
    }

    public override void Render()
    {
        UpdateOrderUI();
    }

    public void GenerateNewOrder()
    {
        if (!Object.HasStateAuthority) return;
        currentOrderBread = breadOptions[Random.Range(0, breadOptions.Length)];
        currentOrderMeat = meatOptions[Random.Range(0, meatOptions.Length)];
        currentOrderCheese = cheeseOptions[Random.Range(0, cheeseOptions.Length)];
        currentPlateStage = 0;
        Debug.Log($"Novo pedido: {currentOrderBread}, {currentOrderMeat}, {currentOrderCheese}");
    }

    private void GenerateNewOrderLocal()
    {
        string bread = breadOptions[Random.Range(0, breadOptions.Length)];
        string meat = meatOptions[Random.Range(0, meatOptions.Length)];
        string cheese = cheeseOptions[Random.Range(0, cheeseOptions.Length)];
        if (orderText != null)
            orderText.text = $"PEDIDO\n\nPao: {bread}\nCarne: {meat}\nQueijo: {cheese}";
        else
            Debug.LogError("orderText é NULL!");
    }

    private void UpdateOrderUI()
    {
        if (orderText == null) return;
        if (currentOrderBread.Length == 0) return;
        orderText.text = $"PEDIDO\n\nPao: {currentOrderBread}\nCarne: {currentOrderMeat}\nQueijo: {currentOrderCheese}";
    }

    // Verifica se o ingrediente é o correto do pedido atual
    public bool IsCorrectIngredient(string ingredientName, RoleType role)
    {
        return (role == RoleType.BreadMaster && ingredientName == currentOrderBread.Value) ||
               (role == RoleType.MeatMaster && ingredientName == currentOrderMeat.Value) ||
               (role == RoleType.CheeseMaster && ingredientName == currentOrderCheese.Value);
    }

    // Retorna o ingrediente atual do pedido para o role
    public string GetCurrentOrder(RoleType role)
    {
        return role switch
        {
            RoleType.BreadMaster => currentOrderBread.Value,
            RoleType.MeatMaster => currentOrderMeat.Value,
            RoleType.CheeseMaster => currentOrderCheese.Value,
            _ => "?"
        };
    }

    public GameObject GetClosestIngredient(Vector3 playerPos, RoleType role)
    {
        GameObject[] targets = role switch
        {
            RoleType.BreadMaster => breadObjects,
            RoleType.MeatMaster => meatObjects,
            RoleType.CheeseMaster => cheeseObjects,
            _ => null
        };
        if (targets == null) return null;

        GameObject closest = null;
        float closestDist = float.MaxValue;
        foreach (GameObject obj in targets)
        {
            if (obj == null) continue;
            float dist = Vector3.Distance(playerPos, obj.transform.position);
            if (dist < closestDist) { closestDist = dist; closest = obj; }
        }
        return closest;
    }

    public void RemoveIngredient(GameObject obj, RoleType role)
    {
        if (role == RoleType.BreadMaster)
            for (int i = 0; i < breadObjects.Length; i++) { if (breadObjects[i] == obj) { breadObjects[i] = null; return; } }
        else if (role == RoleType.MeatMaster)
            for (int i = 0; i < meatObjects.Length; i++) { if (meatObjects[i] == obj) { meatObjects[i] = null; return; } }
        else if (role == RoleType.CheeseMaster)
            for (int i = 0; i < cheeseObjects.Length; i++) { if (cheeseObjects[i] == obj) { cheeseObjects[i] = null; return; } }
    }

    public void TryAddIngredient(string ingredientName, PlayerRef player, RoleType playerRole)
    {
        RPC_RequestAddIngredient(ingredientName, player, (int)playerRole);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAddIngredient(string ingredientName, PlayerRef player, int roleInt)
    {
        RoleType playerRole = (RoleType)roleInt;
        string expectedType = "";

        if (ingredientName == currentOrderBread.Value && playerRole == RoleType.BreadMaster) expectedType = "bread";
        else if (ingredientName == currentOrderMeat.Value && playerRole == RoleType.MeatMaster) expectedType = "meat";
        else if (ingredientName == currentOrderCheese.Value && playerRole == RoleType.CheeseMaster) expectedType = "cheese";
        else { RPC_ShowMessage($"❌ {ingredientName} não é o ingrediente correto!"); return; }

        int expectedStage = expectedType switch { "bread" => 1, "meat" => 2, "cheese" => 3, _ => -1 };

        if (currentPlateStage + 1 != expectedStage)
        {
            RPC_ShowMessage($"Ordem errada! A ordem e: Pao -> Carne -> Queijo");
            return;
        }

        currentPlateStage = expectedStage;
        RPC_ShowMessage($"✅ {ingredientName} adicionado!");

        if (currentPlateStage == 3)
        {
            completedOrders++;
            RPC_OrderComplete(completedOrders);
            GenerateNewOrder();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowMessage(string msg) { Debug.Log(msg); }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OrderComplete(int orders) { Debug.Log($"PEDIDO COMPLETO! Total: {orders}"); }
}