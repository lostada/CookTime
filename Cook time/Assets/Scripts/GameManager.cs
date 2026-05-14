using Fusion;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Opcoes de cada jogador")]
    public string[] breadOptions = { "PaoAustraliano", "PaoBrioche" };
    public string[] meatOptions = { "CalabresaAcebolada", "Frango", "CarneMoida" };
    public string[] cheeseOptions = { "QueijoPrato", "Cheddar" };

    [Header("Ingredientes na Cena")]
    public GameObject[] breadObjects;
    public GameObject[] meatObjects;
    public GameObject[] cheeseObjects;

    [Header("UI")]
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI plateText;
    public TextMeshProUGUI timerText;

    [Header("Timer")]
    public float totalTime = 120f; // tempo total em segundos

    [Networked] public NetworkString<_32> currentOrderBread { get; set; }
    [Networked] public NetworkString<_32> currentOrderMeat { get; set; }
    [Networked] public NetworkString<_32> currentOrderCheese { get; set; }
    [Networked] public int currentPlateStage { get; set; }
    [Networked] public float timeRemaining { get; set; }
    [Networked] public bool gameOver { get; set; }

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
            timeRemaining = totalTime;
            gameOver = false;
            GenerateNewOrder();
            Debug.Log("GenerateNewOrder chamado!");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        if (gameOver) return;

        timeRemaining -= Runner.DeltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            gameOver = true;
            RPC_GameOver(completedOrders);
        }
    }

    public override void Render()
    {
        UpdateOrderUI();
        UpdatePlateUI();
        UpdateTimerUI();
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
            Debug.LogError("orderText e NULL!");
    }

    private void UpdateOrderUI()
    {
        if (orderText == null) return;
        if (currentOrderBread.Length == 0) return;
        orderText.text = $"PEDIDO\n\nPao: {currentOrderBread}\nCarne: {currentOrderMeat}\nQueijo: {currentOrderCheese}";
    }

    private void UpdatePlateUI()
    {
        if (plateText == null) return;
        if (currentOrderBread.Length == 0) return;

        string pao = currentPlateStage >= 1 ? $"{currentOrderBread} [OK]" : "aguardando...";
        string carne = currentPlateStage >= 2 ? $"{currentOrderMeat} [OK]" : "aguardando...";
        string queijo = currentPlateStage >= 3 ? $"{currentOrderCheese} [OK]" : "aguardando...";

        plateText.text = $"PAO\n\nPao: {pao}\nCarne: {carne}\nQueijo: {queijo}";
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int mins = Mathf.FloorToInt(timeRemaining / 60f);
        int secs = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"Tempo: {mins:00}:{secs:00}";

        // Fica vermelho nos ultimos 30 segundos
        timerText.color = timeRemaining <= 30f ? Color.red : Color.white;
    }

    public bool IsCorrectIngredient(string ingredientName, RoleType role)
    {
        return (role == RoleType.BreadMaster && ingredientName == currentOrderBread.Value) ||
               (role == RoleType.MeatMaster && ingredientName == currentOrderMeat.Value) ||
               (role == RoleType.CheeseMaster && ingredientName == currentOrderCheese.Value);
    }

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
        if (gameOver) return;

        if (Object.HasStateAuthority)
            ProcessIngredient(ingredientName, player, (int)playerRole);
        else
            RPC_RequestAddIngredient(ingredientName, player, (int)playerRole);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestAddIngredient(string ingredientName, PlayerRef player, int roleInt)
    {
        ProcessIngredient(ingredientName, player, roleInt);
    }

    private void ProcessIngredient(string ingredientName, PlayerRef player, int roleInt)
    {
        if (gameOver) return;

        RoleType playerRole = (RoleType)roleInt;

        bool added = false;

        if (playerRole == RoleType.BreadMaster &&
            ingredientName == currentOrderBread.Value &&
            currentPlateStage == 0)
        {
            currentPlateStage = 1;
            added = true;
        }
        else if (playerRole == RoleType.MeatMaster &&
                 ingredientName == currentOrderMeat.Value &&
                 currentPlateStage == 1)
        {
            currentPlateStage = 2;
            added = true;
        }
        else if (playerRole == RoleType.CheeseMaster &&
                 ingredientName == currentOrderCheese.Value &&
                 currentPlateStage == 2)
        {
            currentPlateStage = 3;
            added = true;
        }

        if (!added)
        {
            RPC_ShowMessage($"Ingrediente errado ou fora de ordem! Stage atual: {currentPlateStage}");
            return;
        }

        RPC_ShowMessage($"{ingredientName} adicionado! Stage: {currentPlateStage}/3");

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_GameOver(int totalOrders) { Debug.Log($"FIM DE JOGO! Pedidos completos: {totalOrders}"); }
}