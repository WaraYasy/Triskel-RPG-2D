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
    
    [Header("Configuración de Combate")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float damageCooldown = 1.5f;
    [SerializeField] private float separationRadius = 0.8f;
    [SerializeField] private float separationStrength = 5f;
    private float lastDamageTime = 0f;

    [Header("Referencias (Opcional)")]
    [SerializeField] private Animator animator;
    
    // Parámetros del Animator (igual que PlayerAnimator)
    private readonly int horizontalParam = Animator.StringToHash("Horizontal");
    private readonly int verticalParam = Animator.StringToHash("Vertical");
    
    private Transform playerTransform;
    private PlayerLight playerLight;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private Vector2 lastMoveDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Verificar si la habilidad del Lirio está activa
        bool shouldFollow = playerLight != null && playerLight.IsAbilityActive();
        
        if (shouldFollow)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
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
            playerHealth = player.GetComponent<PlayerHealth>();
            
            if (playerLight != null)
            {
                Debug.Log($"[GhostAI] Player y PlayerLight encontrados");
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;
        
        float distance = Vector2.Distance(rb.position, playerTransform.position);
        
        if (distance > detectionRadius || distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
            UpdateAnimation(Vector2.zero);
            return;
        }
        
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        
        // --- Lógica de Separación ---
        Vector2 separation = Vector2.zero;
        Collider2D[] nearbyGhosts = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        int count = 0;
        
        foreach (var col in nearbyGhosts)
        {
            if (col.gameObject != gameObject && col.CompareTag(gameObject.tag))
            {
                Vector2 diff = rb.position - (Vector2)col.transform.position;
                float mag = diff.magnitude;
                if (mag > 0)
                    separation += diff.normalized / mag;
                count++;
            }
        }
        
        Vector2 finalDirection = directionToPlayer;
        if (count > 0)
        {
            finalDirection = (directionToPlayer + (separation * separationStrength)).normalized;
        }

        // Suavizamos el cambio de dirección actual
        lastMoveDirection = Vector2.Lerp(lastMoveDirection, finalDirection, Time.fixedDeltaTime * 10f);
        
        // Movemos usando velocidad para máxima fluidez física
        rb.linearVelocity = lastMoveDirection * moveSpeed;
        
        // --- ANIMACIÓN ---
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            UpdateAnimation(lastMoveDirection);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryDamagePlayer();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer();
        }
    }

    private void TryDamagePlayer()
    {
        Debug.Log($"[GhostAI] Tocando al Player. PlayerHealth es null? {playerHealth == null}");
        if (Time.time >= lastDamageTime + damageCooldown)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform.position);
                lastDamageTime = Time.time;
                Debug.Log($"[GhostAI] ¡Daño causado al Player! Vida restante: {playerHealth.GetCurrentHealth()}");
            }
        }
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;
        
        // Actualizar parámetros del Animator
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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
