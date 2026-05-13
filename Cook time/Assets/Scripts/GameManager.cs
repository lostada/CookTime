using Fusion;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Opções de cada jogador")]
    public string[] breadOptions = { "PaoAustraliano", "PaoBrioche", };
    public string[] meatOptions = { "CalabresaAcebolada", "Frango", "CarneMoida" };
    public string[] cheeseOptions = { "QueijoPrato", "Cheddar" };


    [Header("Ingredientes na Cena")]
    public GameObject[] breadObjects;  // arraste os 2 pães aqui
    public GameObject[] meatObjects;   // arraste as 3 carnes aqui
    public GameObject[] cheeseObjects; // arraste os 2 queijos aqui

    // Pega o ingrediente mais próximo do player baseado no role
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
            if (obj == null) continue; // já foi pego
            float dist = Vector3.Distance(playerPos, obj.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = obj;
            }
        }

        return closest;
    }
    public void RemoveIngredient(GameObject obj, RoleType role)
    {
        System.Collections.Generic.List<GameObject> list = role switch
        {
            RoleType.BreadMaster => new System.Collections.Generic.List<GameObject>(breadObjects),
            RoleType.MeatMaster => new System.Collections.Generic.List<GameObject>(meatObjects),
            RoleType.CheeseMaster => new System.Collections.Generic.List<GameObject>(cheeseObjects),
            _ => null
        };

        if (list == null) return;

        int idx = list.IndexOf(obj);
        if (idx >= 0)
        {
            if (role == RoleType.BreadMaster) breadObjects[idx] = null;
            if (role == RoleType.MeatMaster) meatObjects[idx] = null;
            if (role == RoleType.CheeseMaster) cheeseObjects[idx] = null;
        }
    }
    [Header("UI")]
    public TextMeshProUGUI orderText;

    // ✅ FIX 1: usar NetworkString em vez de string pura
    [Networked] public NetworkString<_32> currentOrderBread { get; set; }
    [Networked] public NetworkString<_32> currentOrderMeat { get; set; }
    [Networked] public NetworkString<_32> currentOrderCheese { get; set; }
    [Networked] public int currentPlateStage { get; set; }

    // ✅ FIX 2: detectar mudança nas networked vars pra atualizar a UI automaticamente
    public override void Render()
    {
        UpdateOrderUI();
    }

    private int completedOrders = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Fallback: se não tiver Fusion rodando, gera pedido local mesmo
        if (Runner == null)
        {
            Debug.Log("Modo offline - gerando pedido local");
            GenerateNewOrderLocal();
        }
    }

    private void GenerateNewOrderLocal()
    {
        // Versão sem Networked, só pra testar UI
        string bread = breadOptions[Random.Range(0, breadOptions.Length)];
        string meat = meatOptions[Random.Range(0, meatOptions.Length)];
        string cheese = cheeseOptions[Random.Range(0, cheeseOptions.Length)];

        if (orderText != null)
            orderText.text = $"📋 PEDIDO\n\n🍞 {bread}\n🥩 {meat}\n🧀 {cheese}";
        else
            Debug.LogError("orderText é NULL!");
    }

    public override void Spawned()
    {
        Debug.Log($"GameManager Spawned! HasStateAuthority: {Object.HasStateAuthority} | HasInputAuthority: {Object.HasInputAuthority}");

        if (Object.HasStateAuthority)
        {
            GenerateNewOrder();
            Debug.Log("GenerateNewOrder chamado!");
        }
        else
        {
            Debug.LogWarning("GameManager NÃO tem StateAuthority! Pedido não foi gerado.");
        }
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

    // ✅ FIX 2: UI atualizada via Render() que roda em todos os clientes
    private void UpdateOrderUI()
    {
        if (orderText == null) return;

        // Só atualiza se já tem dados
        if (currentOrderBread.Length == 0) return;

        orderText.text = $"📋 PEDIDO\n\n🍞 {currentOrderBread}\n🥩 {currentOrderMeat}\n🧀 {currentOrderCheese}";
    }

    // ✅ FIX 3: cliente envia RPC pro servidor em vez de chamar direto
    public void TryAddIngredient(string ingredientName, PlayerRef player, RoleType playerRole)
    {
        RPC_RequestAddIngredient(ingredientName, player, (int)playerRole);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestAddIngredient(string ingredientName, PlayerRef player, int roleInt)
    {
        RoleType playerRole = (RoleType)roleInt;

        bool isCorrect = false;
        string expectedType = "";

        if (ingredientName == currentOrderBread.Value && playerRole == RoleType.BreadMaster)
        { isCorrect = true; expectedType = "bread"; }
        else if (ingredientName == currentOrderMeat.Value && playerRole == RoleType.MeatMaster)
        { isCorrect = true; expectedType = "meat"; }
        else if (ingredientName == currentOrderCheese.Value && playerRole == RoleType.CheeseMaster)
        { isCorrect = true; expectedType = "cheese"; }

        if (!isCorrect)
        {
            RPC_ShowMessage($"❌ {ingredientName} não é o ingrediente correto para {playerRole}!");
            return;
        }

        int expectedStage = expectedType switch
        {
            "bread" => 1,
            "meat" => 2,
            "cheese" => 3,
            _ => -1
        };

        if (currentPlateStage + 1 != expectedStage)
        {
            RPC_ShowMessage($"⚠️ Ordem errada! A ordem é: Pão → Carne → Queijo");
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
    private void RPC_ShowMessage(string msg)
    {
        Debug.Log(msg);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OrderComplete(int orders)
    {
        Debug.Log($"🎉 PEDIDO COMPLETO! Total: {orders}");
    }
}