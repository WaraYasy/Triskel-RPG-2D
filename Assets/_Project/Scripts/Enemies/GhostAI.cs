using UnityEngine;

/// <summary>
/// GhostAI - Fantasma con doble comportamiento frente al Lirio.
/// 1. Sigue al jugador si detecta su luz.
/// 2. Muere si es golpeado por la luz intensa (habilidad Z activa).
/// 3. Se siente atraido por las fuentes de liberacion si esta cerca.
/// </summary>
public class GhostAI : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float stopDistance = 0.5f;
    
    [Header("Configuracion de Combate")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float damageCooldown = 1.5f;
    [SerializeField] private float dissipationDistance = 2.5f; // Distancia a la que muere si Z esta activa
    [SerializeField] private GameObject deathParticles;        // Opcional: Efecto visual de muerte
    
    [Header("Fuentes de Liberacion")]
    [SerializeField] private float fountainDetectionRadius = 5f;
    private Transform activeFountain;

    [Header("Separacion entre fantasmas")]
    [SerializeField] private float separationRadius = 0.8f;
    [SerializeField] private float separationStrength = 5f;
    
    private Transform playerTransform;
    private PlayerLight playerLight;
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private float lastDamageTime = 0f;
    private Vector2 lastMoveDirection = Vector2.down;
    private Animator animator;
    private GhostLiberation liberationComponent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        liberationComponent = GetComponent<GhostLiberation>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (playerTransform == null) FindPlayer();
        
        // Comprobar si debemos morir por uso directo de la luz
        CheckLirioDeath();

        // Buscar fuentes si no estamos yendo a una
        if (activeFountain == null)
        {
            FindNearbyFountain();
        }
    }

    private void FixedUpdate()
    {
        if (liberationComponent != null && liberationComponent.IsBeingLiberated) return;

        // Prioridad 1: Ir a la fuente si hay una cerca
        if (activeFountain != null)
        {
            MoveTowardsTarget(activeFountain.position);
            return;
        }

        // Prioridad 2: Seguir al jugador
        if (playerTransform == null || playerLight == null) return;

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        bool isAttracted = playerLight.HasLirio() && distanceToPlayer <= detectionRadius;

        if (isAttracted)
        {
            MoveTowardsTarget(playerTransform.position);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
        }
    }

    private void FindNearbyFountain()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, fountainDetectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<LiberationFountain>() != null)
            {
                activeFountain = hitCollider.transform;
                Debug.Log("👻 ¡Fantasma atraido por la fuente de liberacion!");
                break;
            }
        }
    }

    private void CheckLirioDeath()
    {
        if (playerLight == null || !playerLight.IsAbilityActive()) return;
        if (liberationComponent != null && liberationComponent.IsBeingLiberated) return;

        float distance = Vector2.Distance(transform.position, playerLight.transform.position);

        if (distance <= dissipationDistance)
        {
            Dissipate();
        }
    }

    private void Dissipate()
    {
        Debug.Log("👻 ¡Fantasma disipado por la luz intensa!");
        
        if (liberationComponent != null)
        {
            liberationComponent.Liberate(GhostLiberation.LiberationMode.Intense);
        }
        else
        {
            if (deathParticles != null) Instantiate(deathParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void MoveTowardsTarget(Vector2 targetPos)
    {
        float distance = Vector2.Distance(rb.position, targetPos);
        
        if (distance <= stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            return;
        }
        
        Vector2 directionToTarget = (targetPos - rb.position).normalized;
        
        // Logica de separacion
        Vector2 separation = Vector2.zero;
        Collider2D[] nearbyGhosts = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        foreach (var col in nearbyGhosts)
        {
            if (col.gameObject != gameObject && col.CompareTag(gameObject.tag))
            {
                Vector2 diff = rb.position - (Vector2)col.transform.position;
                if (diff.magnitude > 0) separation += diff.normalized / diff.magnitude;
            }
        }
        
        Vector2 finalDirection = (directionToTarget + (separation * separationStrength)).normalized;
        lastMoveDirection = Vector2.Lerp(lastMoveDirection, finalDirection, Time.fixedDeltaTime * 10f);
        rb.linearVelocity = lastMoveDirection * moveSpeed;
        
        UpdateAnimation(lastMoveDirection);
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerLight = player.GetComponent<PlayerLight>();
            playerHealth = player.GetComponent<PlayerHealth>();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) TryDamagePlayer();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player")) TryDamagePlayer();
    }

    private void TryDamagePlayer()
    {
        if (playerHealth == null) return;
        if (liberationComponent != null && liberationComponent.IsBeingLiberated) return;

        if (Time.time >= lastDamageTime + damageCooldown)
        {
            playerHealth.TakeDamage(damageAmount, transform.position);
            lastDamageTime = Time.time;
        }
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;
        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dissipationDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fountainDetectionRadius);
    }
}
