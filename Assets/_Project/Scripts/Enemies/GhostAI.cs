using UnityEngine;

/// <summary>
/// GhostAI - Fantasma que se mueve hacia la luz del Lirio
/// Versión 2.0 - Con animaciones 4 direcciones
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
    
    private Transform lirioLightTransform;
    private Vector2 lastMoveDirection = Vector2.down;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        // Buscar la luz del Lirio activa
        FindLirioLight();
        
        // Si hay luz, moverse hacia ella
        if (lirioLightTransform != null)
        {
            MoveTowardsLight();
        }
        else
        {
            // Si no hay luz, animación idle (dirección 0,0)
            UpdateAnimation(Vector2.zero);
        }
    }

    private void FindLirioLight()
    {
        // Buscar objeto con tag "LirioLight"
        GameObject lirioLight = GameObject.FindGameObjectWithTag("LirioLight");
        
        if (lirioLight != null && lirioLight.activeInHierarchy)
        {
            lirioLightTransform = lirioLight.transform;
        }
        else
        {
            lirioLightTransform = null;
        }
    }

    private void MoveTowardsLight()
    {
        if (lirioLightTransform == null) return;
        
        // Calcular distancia
        float distance = Vector2.Distance(transform.position, lirioLightTransform.position);
        
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
        
        // Moverse hacia la luz
        Vector2 direction = ((Vector2)lirioLightTransform.position - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, lirioLightTransform.position, moveSpeed * Time.deltaTime);
        
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
