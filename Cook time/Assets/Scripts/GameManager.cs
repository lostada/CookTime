using Fusion;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Opções de cada jogador")]
    public string[] breadOptions = { "PaoAustraliano", "PaoBrioche", "PaoDeManteiga" };
    public string[] meatOptions = { "CalabresaAcebolada", "Frango", "CarneMoida" };
    public string[] cheeseOptions = { "QueijoPrato", "Mussarela", "Cheddar" };

    [Header("UI")]
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI statusText;

    [Header("Pratos")]
    public GameObject platePrefab;           // Prefab do prato
    public Transform plateSpawnPoint;        // Onde o prato começa (mesa 1)
    public Transform breadTablePlatePoint;   // Posição do prato na mesa do pão
    public Transform meatTablePlatePoint;    // Posição do prato na mesa da carne
    public Transform cheeseTablePlatePoint;  // Posição do prato na mesa do queijo
    public Transform finalPlatePoint;        // Posição final (sanduíche pronto)

    [Header("Prefabs 3D dos Ingredientes")]
    public GameObject[] bread3DPrefabs;      // 3 pães 3D
    public GameObject[] meat3DPrefabs;       // 3 carnes 3D
    public GameObject[] cheese3DPrefabs;     // 3 queijos 3D

    [Header("Referências das Mesas no Cenário")]
    public Transform breadTable;
    public Transform meatTable;
    public Transform cheeseTable;

    [Header("Skins dos Players")]
    public NetworkObject breadSkin;
    public NetworkObject meatSkin;
    public NetworkObject cheeseSkin;

    // Estado do jogo
    [Networked] public string currentOrderBread { get; set; }
    [Networked] public string currentOrderMeat { get; set; }
    [Networked] public string currentOrderCheese { get; set; }
    [Networked] public int currentPlateStage { get; set; } // 0=vazio, 1=temPao, 2=temCarne, 3=temQueijo

    private NetworkObject currentPlate;
    private GameObject currentPlateVisual;
    private int playersConnected = 0;
    private int completedOrders = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            GenerateNewOrder();
            SpawnInitialPlate();
        }
    }

    private void SpawnInitialPlate()
    {
        if (platePrefab != null && plateSpawnPoint != null)
        {
            currentPlate = Runner.Spawn(platePrefab.GetComponent<NetworkObject>(), plateSpawnPoint.position, Quaternion.identity);
            currentPlateStage = 0;
        }
    }

    public void PlayerConnected()
    {
        if (!Object.HasStateAuthority) return;

        playersConnected++;
        RPC_ShowMessage($"Aguardando jogadores... ({playersConnected}/3)", Color.yellow);

        if (playersConnected >= 3)
        {
            RPC_ShowMessage("Todos conectados! Vamos cozinhar!", Color.green);
        }
    }

    public void GenerateNewOrder()
    {
        if (!Object.HasStateAuthority) return;

        currentOrderBread = breadOptions[Random.Range(0, breadOptions.Length)];
        currentOrderMeat = meatOptions[Random.Range(0, meatOptions.Length)];
        currentOrderCheese = cheeseOptions[Random.Range(0, cheeseOptions.Length)];

        currentPlateStage = 0;

        // Reseta o prato
        if (currentPlate != null && currentPlate.IsValid)
        {
            currentPlate.transform.position = plateSpawnPoint.position;
        }

        // Limpa visual do prato
        if (currentPlateVisual != null)
            Destroy(currentPlateVisual);

        RPC_UpdateOrderUI(currentOrderBread, currentOrderMeat, currentOrderCheese);
        RPC_ShowMessage($"📋 NOVO PEDIDO! {currentOrderBread} + {currentOrderMeat} + {currentOrderCheese}", Color.green);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_UpdateOrderUI(string bread, string meat, string cheese)
    {
        if (orderText != null)
        {
            orderText.text = $"📋 PEDIDO\n\n🍞 {bread}\n🥩 {meat}\n🧀 {cheese}";
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowMessage(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
        Debug.Log(message);
    }

    public bool TryAddIngredient(string ingredient, PlayerRef player, RoleType playerRole)
    {
        if (!Object.HasStateAuthority) return false;

        // Verifica se o jogador está na mesa certa
        Transform playerTable = GetTableForRole(playerRole);
        if (playerTable == null) return false;

        // Verifica se o ingrediente é o correto para o pedido
        bool isCorrect = false;
        string ingredientType = "";

        if (ingredient == currentOrderBread && playerRole == RoleType.BreadMaster)
        {
            isCorrect = true;
            ingredientType = "bread";
        }
        else if (ingredient == currentOrderMeat && playerRole == RoleType.MeatMaster)
        {
            isCorrect = true;
            ingredientType = "meat";
        }
        else if (ingredient == currentOrderCheese && playerRole == RoleType.CheeseMaster)
        {
            isCorrect = true;
            ingredientType = "cheese";
        }

        if (!isCorrect)
        {
            RPC_ShowMessage($"❌ {ingredient} não é o ingrediente correto ou você não pode pegar ele!", Color.red);
            return false;
        }

        // Avança o estágio do prato
        int newStage = currentPlateStage + 1;

        if (newStage == 1 && ingredientType == "bread")
        {
            currentPlateStage = 1;
            TeleportPlateToBreadTable();
            AddIngredientToPlate(ingredient, ingredientType);
            RPC_ShowMessage($"✅ {ingredient} adicionado ao prato!", Color.green);
        }
        else if (newStage == 2 && ingredientType == "meat" && currentPlateStage == 1)
        {
            currentPlateStage = 2;
            TeleportPlateToMeatTable();
            AddIngredientToPlate(ingredient, ingredientType);
            RPC_ShowMessage($"✅ {ingredient} adicionado ao prato!", Color.green);
        }
        else if (newStage == 3 && ingredientType == "cheese" && currentPlateStage == 2)
        {
            currentPlateStage = 3;
            TeleportPlateToCheeseTable();
            AddIngredientToPlate(ingredient, ingredientType);
            RPC_ShowMessage($"✅ {ingredient} adicionado ao prato!", Color.green);

            // Pedido completo!
            completedOrders++;
            RPC_OrderComplete(completedOrders);
            GenerateNewOrder();
        }
        else
        {
            RPC_ShowMessage($"⚠️ Não é a vez deste ingrediente! Ordme correta: PÃO → CARNE → QUEIJO", Color.yellow);
            return false;
        }

        return true;
    }

    private void TeleportPlateToBreadTable()
    {
        if (currentPlate != null && breadTablePlatePoint != null)
        {
            currentPlate.transform.position = breadTablePlatePoint.position;
        }
    }

    private void TeleportPlateToMeatTable()
    {
        if (currentPlate != null && meatTablePlatePoint != null)
        {
            currentPlate.transform.position = meatTablePlatePoint.position;
        }
    }

    private void TeleportPlateToCheeseTable()
    {
        if (currentPlate != null && cheeseTablePlatePoint != null)
        {
            currentPlate.transform.position = cheeseTablePlatePoint.position;
        }
    }

    private void AddIngredientToPlate(string ingredientName, string type)
    {
        // Instancia o modelo 3D do ingrediente em cima do prato
        GameObject prefabToSpawn = null;

        if (type == "bread")
        {
            foreach (var bread in bread3DPrefabs)
            {
                if (bread.name.Contains(ingredientName))
                {
                    prefabToSpawn = bread;
                    break;
                }
            }
        }
        else if (type == "meat")
        {
            foreach (var meat in meat3DPrefabs)
            {
                if (meat.name.Contains(ingredientName))
                {
                    prefabToSpawn = meat;
                    break;
                }
            }
        }
        else if (type == "cheese")
        {
            foreach (var cheese in cheese3DPrefabs)
            {
                if (cheese.name.Contains(ingredientName))
                {
                    prefabToSpawn = cheese;
                    break;
                }
            }
        }

        if (prefabToSpawn != null && currentPlate != null)
        {
            GameObject ingredientObj = Instantiate(prefabToSpawn, currentPlate.transform);
            ingredientObj.transform.localPosition = new Vector3(0, 0.2f + (currentPlateStage * 0.1f), 0);
            ingredientObj.transform.localRotation = Quaternion.identity;
            ingredientObj.transform.localScale = Vector3.one * 0.5f;

            if (currentPlateVisual == null)
                currentPlateVisual = ingredientObj;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_OrderComplete(int orders)
    {
        if (statusText != null)
        {
            statusText.text = $"🎉 PEDIDO COMPLETO! Pedidos: {orders} 🎉";
            statusText.color = Color.cyan;
        }
        Debug.Log($"🎉 PEDIDO COMPLETO! Total: {orders}");
    }

    public Transform GetTableForRole(RoleType role)
    {
        switch (role)
        {
            case RoleType.BreadMaster: return breadTable;
            case RoleType.MeatMaster: return meatTable;
            case RoleType.CheeseMaster: return cheeseTable;
            default: return null;
        }
    }

    public NetworkObject GetSkinForRole(RoleType role)
    {
        switch (role)
        {
            case RoleType.BreadMaster: return breadSkin;
            case RoleType.MeatMaster: return meatSkin;
            case RoleType.CheeseMaster: return cheeseSkin;
            default: return null;
        }
    }

    public bool IsPlayerAtCorrectTable(Transform playerPos, RoleType role)
    {
        Transform targetTable = GetTableForRole(role);
        if (targetTable == null) return true;

        return Vector3.Distance(playerPos.position, targetTable.position) < 3f;
    }
}