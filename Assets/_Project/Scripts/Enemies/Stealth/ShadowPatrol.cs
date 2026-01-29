using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ShadowPatrol - Sombra que patrulla y busca al jugador.
/// Mecánica de sigilo para el Cuadrante 3.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ShadowPatrol : MonoBehaviour
{
    [Header("Configuración de Patrulla")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float waitTimeAtPoint = 1f;
    [SerializeField] private bool loopPatrol = true;
    
    [Header("Configuración de Detección")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float detectionAngle = 90f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float chaseDuration = 5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Configuración de Alerta")]
    [SerializeField] private float suspicionTime = 2f;
    [SerializeField] private float alertSearchDuration = 3f;
    [SerializeField] private float hearingRadius = 2f; // Radio de detección por sonido (ej: dash)
    
    [Header("Dificultad Extra")]
    [SerializeField] private float proximityDetectionRadius = 1.5f; // Te detectan si estás MUY cerca
    [SerializeField] private bool lookBackEnabled = true;           // Mirar hacia atrás ocasionalmente
    [SerializeField] private float lookBackChance = 0.3f;           // Probabilidad de mirar atrás (0-1)
    [SerializeField] private float lookBackDuration = 0.8f;         // Duración de la mirada atrás
    
    [Header("Zonas Seguras")]
    [SerializeField] private LayerMask safeZoneLayer;  // Layer de zonas donde la sombra no puede entrar
    [SerializeField] private float safeZoneCheckRadius = 0.5f;
    
    [Header("Indicadores Visuales")]
    [SerializeField] private GameObject detectionIndicator;      // Indicador "!" cuando detecta
    [SerializeField] private GameObject suspicionIndicator;      // Indicador "?" cuando sospecha
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color suspiciousColor = Color.yellow;
    [SerializeField] private Color alertColor = Color.red;
    
    [Header("Daño al Jugador")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float catchDistance = 0.5f;
    
    // Estados
    public enum ShadowState { Patrolling, Suspicious, Chasing, Searching, Returning }
    private ShadowState currentState = ShadowState.Patrolling;
    
    // Referencias
    private Rigidbody2D rb;
    private Animator animator;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private PlayerStealth playerStealth;
    
    // Variables de patrulla
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private bool patrolForward = true;
    
    // Variables de persecución
    private Vector2 lastKnownPlayerPosition;
    private float chaseTimer = 0f;
    private float suspicionTimer = 0f;
    private float searchTimer = 0f;
    
    // Variables de mirada hacia atrás
    private bool isLookingBack = false;
    private Vector2 originalDirection;
    
    // Dirección para animación
    private Vector2 currentDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        FindPlayer();
        UpdateIndicators();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        switch (currentState)
        {
            case ShadowState.Patrolling:
                CheckForPlayer();
                break;
                
            case ShadowState.Suspicious:
                HandleSuspicion();
                break;
                
            case ShadowState.Chasing:
                HandleChase();
                break;
                
            case ShadowState.Searching:
                HandleSearch();
                break;
                
            case ShadowState.Returning:
                // Manejado en FixedUpdate
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case ShadowState.Patrolling:
                if (!isWaiting) MoveToNextPatrolPoint();
                break;
                
            case ShadowState.Chasing:
                ChasePlayer();
                break;
                
            case ShadowState.Searching:
                SearchArea();
                break;
                
            case ShadowState.Returning:
                ReturnToPatrol();
                break;
        }
    }

    #region Detección del Jugador

    private void CheckForPlayer()
    {
        if (IsPlayerHidden()) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // DETECCIÓN POR PROXIMIDAD - Si está muy cerca, lo detecta sin importar el ángulo
        if (distanceToPlayer <= proximityDetectionRadius)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
            
            if (hit.collider == null)
            {
                Debug.Log("👃 ¡Sombra detecta al jugador por proximidad!");
                StartChasing();
                return;
            }
        }
        
        // Detectar por sonido (siempre que esté en rango de audición)
        if (distanceToPlayer <= hearingRadius && IsPlayerMakingNoise())
        {
            BecomeSuspicious(playerTransform.position);
            return;
        }
        
        // Detectar por visión
        if (distanceToPlayer <= detectionRadius)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector2.Angle(currentDirection, directionToPlayer);
            
            if (angleToPlayer <= detectionAngle / 2f)
            {
                // Verificar línea de visión
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
                
                if (hit.collider == null)
                {
                    // ¡Jugador detectado!
                    StartChasing();
                }
            }
        }
    }

    private bool IsPlayerHidden()
    {
        if (playerStealth == null)
        {
            playerStealth = playerTransform.GetComponent<PlayerStealth>();
        }
        return playerStealth != null && playerStealth.IsHidden();
    }

    private bool IsPlayerMakingNoise()
    {
        if (playerStealth == null)
        {
            playerStealth = playerTransform.GetComponent<PlayerStealth>();
        }
        return playerStealth != null && playerStealth.IsMakingNoise();
    }

    #endregion

    #region Estados de Comportamiento

    private void BecomeSuspicious(Vector2 noisePosition)
    {
        if (currentState == ShadowState.Chasing) return;
        
        currentState = ShadowState.Suspicious;
        suspicionTimer = 0f;
        lastKnownPlayerPosition = noisePosition;
        
        UpdateIndicators();
        UpdateColor(suspiciousColor);
        Debug.Log("👁 Sombra sospecha de algo...");
    }

    private void HandleSuspicion()
    {
        suspicionTimer += Time.deltaTime;
        
        // Mirar hacia la posición sospechosa
        Vector2 lookDirection = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
        currentDirection = lookDirection;
        UpdateAnimation(Vector2.zero);
        
        // Si ve al jugador durante la sospecha, empieza a perseguir
        if (!IsPlayerHidden())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRadius)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                float angleToPlayer = Vector2.Angle(currentDirection, directionToPlayer);
                
                if (angleToPlayer <= detectionAngle / 2f)
                {
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
                    if (hit.collider == null)
                    {
                        StartChasing();
                        return;
                    }
                }
            }
        }
        
        // Si pasa el tiempo sin ver nada, volver a patrullar
        if (suspicionTimer >= suspicionTime)
        {
            currentState = ShadowState.Returning;
            UpdateIndicators();
            UpdateColor(normalColor);
        }
    }

    private void StartChasing()
    {
        currentState = ShadowState.Chasing;
        chaseTimer = 0f;
        lastKnownPlayerPosition = playerTransform.position;
        
        UpdateIndicators();
        UpdateColor(alertColor);
        Debug.Log("🔴 ¡Sombra ha detectado al jugador!");
    }

    private void HandleChase()
    {
        // Actualizar última posición conocida si puede ver al jugador
        if (!IsPlayerHidden())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            // Verificar si atrapa al jugador
            if (distanceToPlayer <= catchDistance)
            {
                CatchPlayer();
                return;
            }
            
            // Actualizar posición si aún puede ver
            if (distanceToPlayer <= detectionRadius * 1.5f) // Radio extendido en persecución
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
                
                if (hit.collider == null)
                {
                    lastKnownPlayerPosition = playerTransform.position;
                    chaseTimer = 0f; // Reset timer mientras puede ver
                }
            }
        }
        
        chaseTimer += Time.deltaTime;
        
        // Si pierde de vista por mucho tiempo, empieza a buscar
        if (chaseTimer >= chaseDuration)
        {
            StartSearching();
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
        
        // Verificar si hay una zona segura en la dirección del movimiento
        if (IsApproachingSafeZone(direction))
        {
            // Detenerse en el borde de la zona segura
            rb.linearVelocity = Vector2.zero;
            currentDirection = direction;
            UpdateAnimation(Vector2.zero);
            
            // Empezar a buscar ya que no puede seguir
            chaseTimer += Time.fixedDeltaTime * 3f; // Acelerar pérdida de visión
            return;
        }
        
        rb.linearVelocity = direction * chaseSpeed;
        currentDirection = direction;
        UpdateAnimation(direction);
    }
    
    /// <summary>
    /// Verifica si la sombra está a punto de entrar en una zona segura.
    /// </summary>
    private bool IsApproachingSafeZone(Vector2 moveDirection)
    {
        // Hacer raycast en la dirección del movimiento
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            moveDirection, 
            safeZoneCheckRadius, 
            safeZoneLayer
        );
        
        if (hit.collider != null)
        {
            return true;
        }
        
        // También verificar si ya estamos dentro de una zona segura (no debería pasar)
        Collider2D overlap = Physics2D.OverlapCircle(transform.position, 0.1f, safeZoneLayer);
        return overlap != null;
    }

    private void StartSearching()
    {
        currentState = ShadowState.Searching;
        searchTimer = 0f;
        UpdateIndicators();
        UpdateColor(suspiciousColor);
        Debug.Log("🔍 Sombra buscando al jugador...");
    }

    private void HandleSearch()
    {
        searchTimer += Time.deltaTime;
        
        // Si encuentra al jugador durante la búsqueda
        if (!IsPlayerHidden())
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRadius)
            {
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
                
                if (hit.collider == null)
                {
                    StartChasing();
                    return;
                }
            }
        }
        
        if (searchTimer >= alertSearchDuration)
        {
            currentState = ShadowState.Returning;
            UpdateIndicators();
            UpdateColor(normalColor);
        }
    }

    private void SearchArea()
    {
        // Movimiento aleatorio en área pequeña buscando
        float distanceToLastKnown = Vector2.Distance(transform.position, lastKnownPlayerPosition);
        
        if (distanceToLastKnown > 0.5f)
        {
            Vector2 direction = (lastKnownPlayerPosition - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * patrolSpeed;
            currentDirection = direction;
            UpdateAnimation(direction);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            
            // Girar buscando
            float angle = Mathf.Sin(searchTimer * 3f) * 90f;
            currentDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
            UpdateAnimation(Vector2.zero);
        }
    }

    private void CatchPlayer()
    {
        Debug.Log("💀 ¡Sombra ha atrapado al jugador!");
        
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount, transform.position);
        }
        
        // Volver a patrullar después de atacar
        currentState = ShadowState.Returning;
        UpdateIndicators();
        UpdateColor(normalColor);
    }

    #endregion

    #region Patrulla

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = ((Vector2)targetPoint.position - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, targetPoint.position);

        if (distance < 0.2f)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
        else
        {
            rb.linearVelocity = direction * patrolSpeed;
            currentDirection = direction;
            UpdateAnimation(direction);
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        rb.linearVelocity = Vector2.zero;
        UpdateAnimation(Vector2.zero);
        
        // Mirar hacia atrás ocasionalmente
        if (lookBackEnabled && Random.value <= lookBackChance)
        {
            yield return StartCoroutine(LookBack());
        }
        
        yield return new WaitForSeconds(waitTimeAtPoint);
        
        // Siguiente punto
        if (loopPatrol)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
        else
        {
            if (patrolForward)
            {
                currentPatrolIndex++;
                if (currentPatrolIndex >= patrolPoints.Length)
                {
                    currentPatrolIndex = patrolPoints.Length - 2;
                    patrolForward = false;
                }
            }
            else
            {
                currentPatrolIndex--;
                if (currentPatrolIndex < 0)
                {
                    currentPatrolIndex = 1;
                    patrolForward = true;
                }
            }
        }
        
        isWaiting = false;
    }

    /// <summary>
    /// Hace que la sombra mire hacia atrás brevemente.
    /// </summary>
    private IEnumerator LookBack()
    {
        isLookingBack = true;
        originalDirection = currentDirection;
        
        // Girar 180 grados
        currentDirection = -originalDirection;
        UpdateAnimation(Vector2.zero);
        
        Debug.Log("👀 Sombra mira hacia atrás...");
        
        // Esperar mientras mira atrás (y seguir detectando al jugador)
        float timer = 0f;
        while (timer < lookBackDuration)
        {
            // Comprobar si ve al jugador mientras mira atrás
            if (playerTransform != null && !IsPlayerHidden())
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= detectionRadius)
                {
                    Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
                    float angleToPlayer = Vector2.Angle(currentDirection, directionToPlayer);
                    
                    if (angleToPlayer <= detectionAngle / 2f)
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
                        if (hit.collider == null)
                        {
                            Debug.Log("🔴 ¡Te pillé por detrás!");
                            isLookingBack = false;
                            StartChasing();
                            yield break;
                        }
                    }
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }
        
        // Volver a la dirección original
        currentDirection = originalDirection;
        UpdateAnimation(Vector2.zero);
        isLookingBack = false;
    }

    private void ReturnToPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            currentState = ShadowState.Patrolling;
            return;
        }

        // Encontrar punto de patrulla más cercano
        int closestIndex = 0;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float dist = Vector2.Distance(transform.position, patrolPoints[i].position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestIndex = i;
            }
        }
        
        currentPatrolIndex = closestIndex;
        
        Vector2 direction = ((Vector2)patrolPoints[closestIndex].position - rb.position).normalized;
        float distance = Vector2.Distance(rb.position, patrolPoints[closestIndex].position);
        
        if (distance < 0.5f)
        {
            currentState = ShadowState.Patrolling;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = direction * patrolSpeed;
            currentDirection = direction;
            UpdateAnimation(direction);
        }
    }

    #endregion

    #region Utilidades

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
            playerStealth = player.GetComponent<PlayerStealth>();
        }
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (animator == null) return;
        
        animator.SetFloat("Horizontal", currentDirection.x);
        animator.SetFloat("Vertical", currentDirection.y);
        
        // Ajustar velocidad de la animación según el movimiento
        float animSpeed = direction.magnitude > 0.1f ? 1f + (direction.magnitude * 0.5f) : 1f;
        animator.speed = animSpeed;
    }

    private void UpdateIndicators()
    {
        if (detectionIndicator != null)
            detectionIndicator.SetActive(currentState == ShadowState.Chasing);
            
        if (suspicionIndicator != null)
            suspicionIndicator.SetActive(currentState == ShadowState.Suspicious || currentState == ShadowState.Searching);
    }

    private void UpdateColor(Color color)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    public ShadowState GetCurrentState() => currentState;

    /// <summary>
    /// Llamar cuando el jugador haga ruido para alertar sombras cercanas.
    /// </summary>
    public void AlertToNoise(Vector2 noisePosition)
    {
        float distance = Vector2.Distance(transform.position, noisePosition);
        if (distance <= hearingRadius && currentState == ShadowState.Patrolling)
        {
            BecomeSuspicious(noisePosition);
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        // Radio de detección visual
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Cono de visión
        Vector3 leftBoundary = Quaternion.Euler(0, 0, detectionAngle / 2f) * (Vector3)currentDirection * detectionRadius;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -detectionAngle / 2f) * (Vector3)currentDirection * detectionRadius;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Radio de audición
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
        
        // Puntos de patrulla
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }
            
            if (loopPatrol && patrolPoints.Length > 1 && patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }
    }

    #endregion
}
