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
    
    [Header("Revelación de la Verdad")]
    [Tooltip("Sprite del animal real (se muestra cuando se revela la verdad). Opcional si usas Animator.")]
    [SerializeField] private Sprite revealedSprite;
    [Tooltip("Animator Controller del animal revelado (opcional, para animaciones del animal)")]
    [SerializeField] private RuntimeAnimatorController revealedAnimatorController;
    [Tooltip("Color del animal revelado")]
    [SerializeField] private Color revealedColor = Color.white;
    [Tooltip("Si está revelado, huye del jugador en vez de atacar")]
    [SerializeField] private float fleeSpeed = 3f;
    [SerializeField] private float fleeDistance = 5f;
    
    [Header("Daño al Jugador")]
    [SerializeField] private float damageAmount = 1f;
    [SerializeField] private float catchDistance = 0.5f;
    
    // Estado de revelación
    private bool isRevealed = false;
    private Sprite originalSprite;
    private RuntimeAnimatorController originalAnimatorController;
    
    // Estados
    public enum ShadowState { Patrolling, Suspicious, Chasing, Searching, Returning }
    private ShadowState currentState = ShadowState.Patrolling;
    
    // Referencias
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D myCollider;
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
    private Vector2 originalDirection;
    
    // Dirección para animación
    private Vector2 currentDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Configuración inicial: Sombra es Fantasma (Trigger)
        if (myCollider != null) myCollider.isTrigger = true;
    }

    private void Start()
    {
        FindPlayer();
        UpdateIndicators();
    }

    private void Update()
    {
        // Si está revelado, solo ejecuta el comportamiento de huida (el cual es una corrutina),
        // así que no hacemos nada en Update para evitar interferencias
        if (isRevealed) return;

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
        // Si está revelado, el movimiento lo controla la corrutina FleeFromPlayerBehavior
        if (isRevealed) return;

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
        // Los animales revelados no detectan agresivamente al jugador
        if (isRevealed) return;
        if (IsPlayerHidden()) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // DETECCIÓN POR PROXIMIDAD - Si está muy cerca, lo detecta sin importar el ángulo
        if (distanceToPlayer <= proximityDetectionRadius)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            RaycastHit2D hit = SafeRaycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
            
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
                RaycastHit2D hit = SafeRaycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
                
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
        // Seguridad extra: no hacer daño si ya somos un animal
        if (isRevealed) return;

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
        
        // Parámetro para saber si se mueve (Solo para el Animal Revelado)
        if (isRevealed)
        {
            bool isMoving = direction.magnitude > 0.01f;
            animator.SetBool("IsMoving", isMoving);
            
            // Ajustar velocidad de la animación según el movimiento
            float animSpeed = isMoving ? 1f + (direction.magnitude * 0.5f) : 1f;
            animator.speed = animSpeed;
        }
        else
        {
            // Lógica original para la Sombra
             float animSpeed = direction.magnitude > 0.1f ? 1f + (direction.magnitude * 0.5f) : 1f;
             animator.speed = animSpeed;
        }
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
        // No cambiar color si ya fue revelado (mantiene el color del animal)
        if (isRevealed) return;

        if (spriteRenderer != null)
            spriteRenderer.color = color;
    }

    public ShadowState GetCurrentState() => currentState;
    
    /// <summary>
    /// Devuelve true si la sombra ha sido revelada como animal
    /// </summary>
    public bool IsRevealed() => isRevealed;

    /// <summary>
    /// Llamar cuando el jugador haga ruido para alertar sombras cercanas.
    /// </summary>
    public void AlertToNoise(Vector2 noisePosition)
    {
        // Los animales revelados no responden a ruidos agresivamente
        if (isRevealed) return;
        
        float distance = Vector2.Distance(transform.position, noisePosition);
        if (distance <= hearingRadius && currentState == ShadowState.Patrolling)
        {
            BecomeSuspicious(noisePosition);
        }
    }
    
    #endregion
    
    #region Revelación de la Verdad
    
    /// <summary>
    /// Revela la verdadera forma de la sombra (un animal asustado).
    /// Llamado por TruthAltar cuando el jugador activa el altar con el Manto.
    /// </summary>
    public void RevealTrueForm()
    {
        if (isRevealed) return;
        isRevealed = true;
        
        Debug.Log($"🐾 {gameObject.name} revela su verdadera forma: ¡Es un animal asustado!");
        
        // Guardar referencias originales
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        if (animator != null)
        {
            originalAnimatorController = animator.runtimeAnimatorController;
        }
        
        // Cambiar al animator del animal si está configurado
        if (revealedAnimatorController != null && animator != null)
        {
            animator.runtimeAnimatorController = revealedAnimatorController;
            Debug.Log($"🐾 {gameObject.name} cambiando a animaciones de animal");
        }
        // Si no hay animator, usar sprite estático
        else if (revealedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = revealedSprite;
        }
        
        // Aplicar color del animal revelado
        if (spriteRenderer != null)
        {
            spriteRenderer.color = revealedColor;
        }
        
        // Ocultar indicadores de amenaza
        if (detectionIndicator != null) detectionIndicator.SetActive(false);
        if (suspicionIndicator != null) suspicionIndicator.SetActive(false);
        
        // CAMBIO IMPORTANTE: Ahora el animal es sólido (choca con árboles)
        if (myCollider != null) myCollider.isTrigger = false;
        
        // Detener cualquier persecución
        currentState = ShadowState.Patrolling;
        rb.linearVelocity = Vector2.zero;
        
        // Iniciar comportamiento de huida
        StartCoroutine(FleeFromPlayerBehavior());
    }
    
    /// <summary>
    /// Comportamiento de animal: Huir si el jugador se acerca, deambular si está lejos.
    /// </summary>
    private System.Collections.IEnumerator FleeFromPlayerBehavior()
    {
        float wanderTimer = 0f;
        float wanderInterval = 2f; // Cambiar dirección cada 2s al deambular
        Vector2 wanderDirection = Vector2.zero;

        while (isRevealed)
        {
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                
                // --- CASO 1: HUIR (Prioridad Alta) ---
                if (distanceToPlayer < fleeDistance)
                {
                    Vector2 desiredFleeDir = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
                    
                    // Usar "Bigotes" para esquivar obstáculos
                    Vector2 smartDir = GetSmartDirection(desiredFleeDir);
                    
                    // Moverse
                    rb.linearVelocity = smartDir * fleeSpeed;
                    currentDirection = smartDir;
                    UpdateAnimation(smartDir);
                    
                    // Resetear timer de deambular
                    wanderTimer = wanderInterval; 
                }
                // --- CASO 2: DEAMBULAR (Idle) ---
                else
                {
                    wanderTimer += Time.deltaTime;
                    
                    if (wanderTimer >= wanderInterval)
                    {
                        wanderTimer = 0f;
                        wanderInterval = Random.Range(2f, 5f);
                        
                        if (Random.value > 0.5f)
                        {
                            wanderDirection = Random.insideUnitCircle.normalized;
                        }
                        else
                        {
                            wanderDirection = Vector2.zero;
                        }
                    }
                    
                    // También aplicar esquiva suave al deambular si se mueve
                    if (wanderDirection != Vector2.zero)
                    {
                        Vector2 smartWander = GetSmartDirection(wanderDirection);
                        rb.linearVelocity = smartWander * (patrolSpeed * 0.5f);
                        currentDirection = smartWander;
                        UpdateAnimation(smartWander);
                    }
                    else
                    {
                        rb.linearVelocity = Vector2.zero;
                        UpdateAnimation(Vector2.zero);
                    }
                }
            }
            
            yield return null; // Esperar al siguiente frame
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

    #region Helpers Anti-Bug
    
    // Método seguro para lanzar rayos sin chocarse con uno mismo
    private RaycastHit2D SafeRaycast(Vector2 origin, Vector2 direction, float distance, LayerMask layerMask)
    {
        bool wasEnabled = false;
        
        // Apagar collider momentáneamente
        if (myCollider != null)
        {
            wasEnabled = myCollider.enabled;
            myCollider.enabled = false;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
        
        // Encender collider de nuevo
        if (myCollider != null)
        {
            myCollider.enabled = wasEnabled;
        }
        
        return hit;
    }
    
    /// <summary>
    /// Calcula una dirección segura basándose en "bigotes" (raycasts) frontales y laterales.
    /// Evita que el animal se choque o se atasque.
    /// </summary>
    private Vector2 GetSmartDirection(Vector2 desiredDir)
    {
        float feelerDist = 1.5f; // Distancia de detección
        
        // 1. Rayo Central
        RaycastHit2D hitCenter = SafeRaycast(transform.position, desiredDir, feelerDist, obstacleLayer);
        
        // 2. Rayo Izquierdo (~45 grados)
        Vector2 leftDir = Quaternion.Euler(0, 0, 45) * desiredDir;
        RaycastHit2D hitLeft = SafeRaycast(transform.position, leftDir, feelerDist * 0.8f, obstacleLayer);
        
        // 3. Rayo Derecho (~-45 grados)
        Vector2 rightDir = Quaternion.Euler(0, 0, -45) * desiredDir;
        RaycastHit2D hitRight = SafeRaycast(transform.position, rightDir, feelerDist * 0.8f, obstacleLayer);

        // --- LÓGICA DE DECISIÓN ---
        
        // Si todo está despejado, seguir recto
        if (hitCenter.collider == null && hitLeft.collider == null && hitRight.collider == null)
        {
            return desiredDir;
        }
        
        // Si el centro está bloqueado
        if (hitCenter.collider != null)
        {
            // Intentar ir por los lados
            if (hitLeft.collider == null && hitRight.collider == null)
            {
                // Ambos lados libres: elegir aleatoriamente para evitar patrones repetitivos
                return Random.value > 0.5f ? leftDir : rightDir;
            }
            else if (hitLeft.collider == null) return leftDir;  // Solo izquierda libre
            else if (hitRight.collider == null) return rightDir; // Solo derecha libre
            else
            {
                 // ¡Todo bloqueado! (Callejón sin salida) -> Dar media vuelta (Pánico)
                 return -desiredDir;
            }
        }
        
        // El centro está libre, pero quizás rozamos una pared lateral
        if (hitLeft.collider != null) return rightDir; // Pared a la izquierda -> empujar derecha
        if (hitRight.collider != null) return leftDir; // Pared a la derecha -> empujar izquierda
        
        return desiredDir;
    }
    
    #endregion

    #endregion
}
