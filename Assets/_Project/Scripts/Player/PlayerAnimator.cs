using UnityEngine;

/// <summary>
/// PlayerAnimator - Controla las animaciones del jugador según dirección y movimiento
/// Versión 1.0 - Animaciones 4 direcciones (andar + idle)
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private RelicSystem relicSystem;
    
    // Parámetros del Animator
    private readonly int speedParam = Animator.StringToHash("Speed");
    private readonly int horizontalParam = Animator.StringToHash("Horizontal");
    private readonly int verticalParam = Animator.StringToHash("Vertical");
    private readonly int lastHorizontalParam = Animator.StringToHash("LastHorizontal");
    private readonly int lastVerticalParam = Animator.StringToHash("LastVertical");
    private readonly int equippedItemParam = Animator.StringToHash("EquippedItem");
    private readonly int attackTriggerParam = Animator.StringToHash("Attack"); // Nuevo parámetro para el ataque
    
    private Vector2 lastMoveDirection = Vector2.down; // Dirección por defecto

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        relicSystem = GetComponent<RelicSystem>();
    }
    
    private void Start()
    {
        // Inicializar con dirección hacia abajo (frente)
        animator.SetFloat(lastHorizontalParam, 0f);
        animator.SetFloat(lastVerticalParam, -1f);
        animator.SetFloat(speedParam, 0f);
        animator.SetFloat(equippedItemParam, 0f);
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null || rb == null) return;
        
        // Actualizar item equipado (0: Nada, 1: Lirio, 2: Hacha, 3: Manto)
        // Actualizar item equipado
        // FIX: Solo el Hacha (2) tiene animación de sostener. Manto (3) y Lirio (1) deben usar la animación base (0).
        if (relicSystem != null)
        {
            float paramValue = 0f;
            if (relicSystem.GetCurrentRelic() == RelicSystem.RelicType.HachaSagrada)
            {
                paramValue = (float)RelicSystem.RelicType.HachaSagrada; // 2.0f
            }
            animator.SetFloat(equippedItemParam, paramValue);
        }

        // Obtener velocidad REAL del Rigidbody
        Vector2 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;
        
        // Si está en movimiento (velocidad > umbral)
        if (speed > 0.1f)
        {
            // Normalizar dirección
            Vector2 moveDirection = velocity.normalized;
            lastMoveDirection = moveDirection;
            
            // IMPORTANTE: Actualizar PRIMERO los Last values
            animator.SetFloat(lastHorizontalParam, moveDirection.x);
            animator.SetFloat(lastVerticalParam, moveDirection.y);
            
            // Luego los valores actuales
            animator.SetFloat(horizontalParam, moveDirection.x);
            animator.SetFloat(verticalParam, moveDirection.y);
        }
        else
        {
            // Si está quieto
            animator.SetFloat(horizontalParam, 0);
            animator.SetFloat(verticalParam, 0);
            
            // NO tocar Last values aquí - mantener los últimos conocidos
        }
        
        // Actualizar velocidad AL FINAL
        animator.SetFloat(speedParam, speed);
    }
    
    public void TriggerAttack()
    {
        animator.SetTrigger(attackTriggerParam);
    }
}

