using UnityEngine;

/// <summary>
/// GhostAI - Fantasma que se mueve hacia el jugador
/// Versión 4.0 - Solo sigue al player cuando usa habilidad del Lirio (Z)
/// </summary>
public class GhostAI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float stopDistance = 0.5f;
    
    [Header("Referencias (Opcional)")]
    [SerializeField] private Animator animator;
    
    // Parámetros del Animator (igual que PlayerAnimator)
    private readonly int horizontalParam = Animator.StringToHash("Horizontal");
    private readonly int verticalParam = Animator.StringToHash("Vertical");
    
    private Transform playerTransform;
    private PlayerLight playerLight;
    private Vector2 lastMoveDirection = Vector2.down;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        // Buscar jugador una vez al inicio
        FindPlayer();
    }

    private void Update()
    {
        // Si no hay player, buscarlo
        if (playerTransform == null)
        {
            FindPlayer();
        }
        
        // Verificar si la habilidad del Lirio está activa
        bool shouldFollow = playerLight != null && playerLight.IsAbilityActive();
        
        // Si hay player Y la habilidad está activa, seguirlo
        if (playerTransform != null && shouldFollow)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Si no hay habilidad activa, quedarse quieto
            UpdateAnimation(Vector2.zero);
        }
    }

    private void FindPlayer()
    {
        // Buscar objeto con tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerTransform = player.transform;
            playerLight = player.GetComponent<PlayerLight>();
            
            if (playerLight != null)
            {
                Debug.Log($"[GhostAI] Player y PlayerLight encontrados");
            }
            else
            {
                Debug.LogWarning("[GhostAI] Player encontrado pero sin PlayerLight component");
            }
        }
        else
        {
            Debug.LogWarning("[GhostAI] No se encontró Player con tag 'Player'");
        }
    }

    private void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        // Calcular distancia
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        
        // Solo moverse si está dentro del radio de detección
        if (distance > detectionRadius)
        {
            UpdateAnimation(Vector2.zero); // Idle
            return;
        }
        
        // Si está muy cerca, detenerse
        if (distance <= stopDistance)
        {
            UpdateAnimation(Vector2.zero); // Idle
            return;
        }
        
        // Moverse hacia el player
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        
        // Actualizar animación con dirección
        lastMoveDirection = direction;
        UpdateAnimation(direction);
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null)
        {
            Debug.LogWarning("[GhostAI] Animator es null!");
            return;
        }
        
        // DEBUG: Ver valores que se envían
        Debug.Log($"[GhostAI] Enviando al Animator: H={direction.x:F2}, V={direction.y:F2}");
        
        // Actualizar parámetros del Animator (igual que PlayerAnimator)
        animator.SetFloat(horizontalParam, direction.x);
        animator.SetFloat(verticalParam, direction.y);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar radio de detección en el editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
