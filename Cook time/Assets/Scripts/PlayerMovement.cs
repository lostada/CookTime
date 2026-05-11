using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Configurações de Movimento")]
    public float walkSpeed = 5f;
    public float mouseSensitivity = 2f;

    [Header("Configurações da Câmera")]
    public float cameraHeight = 1.6f;
    public float minLookAngle = -90f;
    public float maxLookAngle = 90f;

    // Componentes
    private CharacterController controller;
    public Camera playerCamera;

    // Controle da câmera
    private float xRotation = 0f;

    // Controle de movimento
    private Vector3 moveDirection;

    private void Awake()
    {
        // --- CONFIGURA O CHARACTER CONTROLLER ---
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        // Configurações padrão do CharacterController
        controller.height = 1.8f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0, 0.9f, 0);

        Debug.Log("CharacterController configurado!");
    }

    public override void Spawned()
    {
        // Só executa se for o dono deste jogador
        if (Object.HasInputAuthority)
        {
            // --- CRIA A CÂMERA ---
            CreateCamera();

            // --- TRAVA O MOUSE ---
            LockCursor();

            Debug.Log("PlayerMovement inicializado - camera criada e mouse travado");
        }
    }

    private void CreateCamera()
    {
        // Procura por uma câmera existente
        playerCamera = GetComponentInChildren<Camera>();

        // Se não existir, cria uma nova
        if (playerCamera == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.parent = transform;
            camObj.transform.localPosition = new Vector3(0, cameraHeight, 0);
            camObj.transform.localRotation = Quaternion.identity;

            playerCamera = camObj.AddComponent<Camera>();
            playerCamera.tag = "MainCamera";

            Debug.Log("Câmera criada!");
        }
        else
        {
            // Se já existe, só ajusta a posição
            playerCamera.transform.localPosition = new Vector3(0, cameraHeight, 0);
            Debug.Log("Câmera encontrada e posicionada!");
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void FixedUpdateNetwork()
    {
        // Só processa se for o dono do personagem
        if (!Object.HasInputAuthority) return;

        // Só processa se os componentes existirem
        if (controller == null || playerCamera == null) return;

        // --- MOVIMENTO ---
        HandleMovement();

        // --- ROTAÇÃO DA CÂMERA ---
        HandleCameraRotation();
    }

    private void HandleMovement()
    {
        // Pega os inputs do teclado
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Cria o vetor de movimento baseado na direção que o personagem está olhando
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (inputDirection.magnitude > 0.1f)
        {
            // Calcula a direção do movimento
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            moveDirection = (forward * inputDirection.z + right * inputDirection.x).normalized;
            moveDirection.y = 0; // Remove movimento vertical (sem pular)

            // Aplica o movimento
            controller.Move(moveDirection * walkSpeed * Runner.DeltaTime);
        }
    }

    private void HandleCameraRotation()
    {
        // Pega os inputs do mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotação horizontal (no eixo Y do personagem)
        transform.Rotate(0, mouseX, 0);

        // Rotação vertical (no eixo X da câmera)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        // Aplica a rotação na câmera
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }

    private void Update()
    {
        // Só processa se for o dono do personagem
        if (!Object.HasInputAuthority) return;

        // Tecla ESC para liberar/travar o mouse
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }
    }

    // DEBUG: Mostra no console se está funcionando
    private void OnEnable()
    {
        Debug.Log("PlayerMovement ativado");
    }
}