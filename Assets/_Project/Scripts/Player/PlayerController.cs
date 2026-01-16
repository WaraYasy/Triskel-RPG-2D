using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador del jugador - Movimiento en 2D con Dash
/// Versión 3.0 - REFACTORIZADO para Input System
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Configuración de Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.5f;
    
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private PlayerInputActions inputActions;
    
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down;
    
    // Estado del Dash
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        
        // Crear instancia de Input Actions
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        // Habilitar Input Actions
        inputActions.Enable();
        
        // Suscribirse a eventos
        inputActions.Player.Dash.performed += OnDashPerformed;
    }

    private void OnDisable()
    {
        // Desuscribirse de eventos
        inputActions.Player.Dash.performed -= OnDashPerformed;
        
        // Deshabilitar Input Actions
        inputActions.Disable();
    }

    private void Update()
    {
        // No permitir input durante dash
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
            return;
        }
        
        // Leer input de movimiento (funciona para teclado, gamepad, móvil)
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        
        // Guardar última dirección (para dash)
        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }
        
        // Cooldown del dash
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Durante el dash, mantener la velocidad del dash
            rb.linearVelocity = lastMoveDirection * dashSpeed;
        }
        else
        {
            // Movimiento normal
            rb.linearVelocity = moveInput.normalized * moveSpeed;
        }
    }
    
    // Callback del Input System para Dash
    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        if (dashCooldownTimer <= 0)
        {
            StartDash();
        }
    }
    
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        // Aplicar velocidad de dash inmediatamente
        rb.linearVelocity = lastMoveDirection * dashSpeed;
    }
    
    // Métodos públicos para UI/Debug
    public bool IsDashing() => isDashing;
    public float GetDashCooldownProgress() => 1f - (dashCooldownTimer / dashCooldown);
    public Vector2 GetLastMoveDirection() => lastMoveDirection;
    public Vector2 GetMoveInput() => moveInput;
}
