using UnityEngine;

/// <summary>
/// PlayerAnimator - Controla las animaciones del jugador según dirección y movimiento
/// Versión 1.0 - Animaciones 4 direcciones (andar + idle)
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    
    // Parámetros del Animator
    private readonly int speedParam = Animator.StringToHash("Speed");
    private readonly int horizontalParam = Animator.StringToHash("Horizontal");
    private readonly int verticalParam = Animator.StringToHash("Vertical");
    private readonly int lastHorizontalParam = Animator.StringToHash("LastHorizontal");
    private readonly int lastVerticalParam = Animator.StringToHash("LastVertical");
    
    private Vector2 lastMoveDirection = Vector2.down; // Dirección por defecto

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (playerController == null) return;
        
        // Obtener dirección de movimiento del PlayerController
        Vector2 moveDirection = playerController.GetLastMoveDirection();
        float speed = moveDirection.magnitude;
        
        // Si está en movimiento, actualizar dirección
        if (speed > 0.01f)
        {
            lastMoveDirection = moveDirection;
            
            // Parámetros de movimiento
            animator.SetFloat(horizontalParam, moveDirection.x);
            animator.SetFloat(verticalParam, moveDirection.y);
            
            // Actualizar última dirección
            animator.SetFloat(lastHorizontalParam, moveDirection.x);
            animator.SetFloat(lastVerticalParam, moveDirection.y);
        }
        else
        {
            // Si está quieto, mantener la dirección en 0
            animator.SetFloat(horizontalParam, 0);
            animator.SetFloat(verticalParam, 0);
            
            // Pero mantener la última dirección para idle
            // (ya está guardada en lastHorizontal/lastVertical)
        }
        
        // Parámetro de velocidad (0 = idle, >0 = andando)
        animator.SetFloat(speedParam, speed);
    }
}
