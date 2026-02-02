using UnityEngine;
using Triskel.API;

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
    
    [Header("Aura de Ralentizacion")]
    [Tooltip("Radio del aura que ralentiza al jugador")]
    [SerializeField] private float slowAuraRadius = 3f;
    [Tooltip("Factor de velocidad cuando el jugador esta en el aura sin Lirio (0.15 = 15% velocidad)")]
    [SerializeField] private float slowdownFactor = 0.15f;
    
    private Transform playerTransform;
    private PlayerLight playerLight;
    private PlayerHealth playerHealth;
    private RelicSystem relicSystem;
    private PlayerController playerController;
    private Rigidbody2D rb;
    private float lastDamageTime = 0f;
    private Vector2 lastMoveDirection = Vector2.down;
    private Animator animator;
    private GhostLiberation liberationComponent;
    private bool isSlowingPlayer = false;
    
    // Sistema estatico para manejar multiples fantasmas afectando al jugador
    private static int ghostsSlowingPlayer = 0;

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
        
        // Aplicar aura de ralentizacion
        CheckSlowAura();
    }
    
    private void CheckSlowAura()
    {
        if (playerTransform == null || playerController == null) return;
        if (liberationComponent != null && liberationComponent.IsBeingLiberated) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool playerInAura = distanceToPlayer <= slowAuraRadius;
        bool lirioProtects = relicSystem != null && relicSystem.IsLirioSelected();
        
        // Si el jugador esta en el aura y NO tiene el Lirio seleccionado
        if (playerInAura && !lirioProtects)
        {
            if (!isSlowingPlayer)
            {
                isSlowingPlayer = true;
                ghostsSlowingPlayer++;
                playerController.SetSpeedMultiplier(slowdownFactor);
                Debug.Log($"\ud83d\udc7b Aura de frio activa - Jugador ralentizado ({ghostsSlowingPlayer} fantasmas)");
            }
        }
        else
        {
            if (isSlowingPlayer)
            {
                isSlowingPlayer = false;
                ghostsSlowingPlayer = Mathf.Max(0, ghostsSlowingPlayer - 1);
                
                // Solo restaurar velocidad si no hay otros fantasmas afectando
                if (ghostsSlowingPlayer == 0)
                {
                    playerController.ResetSpeedMultiplier();
                    Debug.Log("\ud83d\udc7b Aura de frio desactivada - Velocidad restaurada");
                }
            }
        }
    }
    
    private void OnDestroy()
    {
        // Limpiar al destruirse
        if (isSlowingPlayer && playerController != null)
        {
            isSlowingPlayer = false;
            ghostsSlowingPlayer = Mathf.Max(0, ghostsSlowingPlayer - 1);
            if (ghostsSlowingPlayer == 0)
            {
                playerController.ResetSpeedMultiplier();
            }
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
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        
        // Solo seguir si el Lirio está SELECCIONADO (en la mano), no solo en inventario
        bool isLirioSelected = relicSystem != null && relicSystem.IsLirioSelected();
        bool isAttracted = isLirioSelected && distanceToPlayer <= detectionRadius;

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
        
        // Penalización moral y registro de decisión (Mala elección: Forzar al espíritu)
        if (GameManager.Instance != null && GameManager.Instance.CurrentLevel == 1)
        {
            GameManager.Instance.ModifyMoralWithChoice(-1, APIConstants.Choices.FORZAR);
        }

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
            relicSystem = player.GetComponent<RelicSystem>();
            playerController = player.GetComponent<PlayerController>();
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
        // Aura de ralentizacion (azul oscuro)
        Gizmos.color = new Color(0.2f, 0.2f, 0.8f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, slowAuraRadius);
    }
}
