using UnityEngine;

/// <summary>
/// Controlador básico del jugador - Movimiento en 2D con Dash
/// Versión 2.0 - Movimiento + Dash con cooldown
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Configuración de Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.down; // Para dash sin moverse
    
    // Estado del Dash
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        
        // Leer input del teclado
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D o Flechas
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S o Flechas
        
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
        
        // Input de Dash (Space)
        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0)
        {
            StartDash();
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
}

