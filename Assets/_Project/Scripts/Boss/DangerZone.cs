using UnityEngine;
using System.Collections;

/// <summary>
/// DangerZone - Zona de peligro que avisa antes de explotar
/// Versión 3.0 - Usa GameConstants para colores y dimensiones
/// </summary>
public class DangerZone : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameConstants gameConstants; // Referencia al ScriptableObject
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float explosionRadius = 2f;
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D damageCollider;
    private bool hasExploded = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageCollider = GetComponent<CircleCollider2D>();
        
        if (damageCollider != null)
        {
            damageCollider.enabled = false;
        }
    }

    public void Initialize(Vector3 position, float radius, float warningTime = 1.5f)
    {
        transform.position = position;
        explosionRadius = radius;
        warningDuration = warningTime;
        
        // Configurar tamaño visual
        transform.localScale = Vector3.one * radius * 2f;
        
        // Color inicial
        if (spriteRenderer != null && gameConstants != null)
        {
            spriteRenderer.color = gameConstants.dangerZoneWarning;
        }
        
        // Configurar collider
        if (damageCollider != null)
        {
            damageCollider.radius = 0.5f;
        }
        
        StartCoroutine(WarningSequence());
    }

    private IEnumerator WarningSequence()
    {
        if (gameConstants == null)
        {
            Debug.LogError("GameConstants no asignado en DangerZone!");
            yield break;
        }
        
        // FASE 1: Parpadeo NARANJA (60% del tiempo)
        float phase1Duration = warningDuration * 0.6f;
        float elapsed = 0f;
        
        while (elapsed < phase1Duration)
        {
            elapsed += Time.deltaTime;
            
            // Parpadeo naranja
            float blink = Mathf.PingPong(Time.time * 6f, 1f);
            Color currentColor = gameConstants.dangerZoneWarning;
            currentColor.a = Mathf.Lerp(0.2f, 0.6f, blink);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = currentColor;
            }
            
            yield return null;
        }
        
        // FASE 2: ROJO SÓLIDO BRILLANTE (40% del tiempo)
        float phase2Duration = warningDuration - phase1Duration;
        elapsed = 0f;
        
        while (elapsed < phase2Duration)
        {
            elapsed += Time.deltaTime;
            
            // ROJO SIN PARPADEO
            if (spriteRenderer != null)
            {
                spriteRenderer.color = gameConstants.dangerZoneDanger;
            }
            
            yield return null;
        }
        
        // FASE 3: Explosión
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        Debug.Log("💥 DangerZone EXPLOTÓ!");
        
        // Activar collider para daño
        if (damageCollider != null)
        {
            damageCollider.enabled = true;
        }
        
        // Iniciar corrutina de destrucción
        StartCoroutine(DestroyAfterFrame());
    }
    
    private IEnumerator DestroyAfterFrame()
    {
        // Esperar un frame para que el collider pueda detectar al player
        yield return new WaitForEndOfFrame();
        
        // Ocultar sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Destruir en el siguiente frame
        yield return null;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasExploded) return;
        
        if (collision.CompareTag("Player"))
        {
            Debug.Log("💥 ¡Player golpeado!");
        }
    }
}

