using UnityEngine;
using System.Collections;

/// <summary>
/// DangerZone - Zona de peligro que avisa antes de explotar
/// Versión 3.0 - Usa GameConstants para colores y dimensiones
/// </summary>
public class DangerZone : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private GameConstants gameConstants;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float explosionRadius = 2f;

    [Header("Efectos de Naturaleza")]
    [SerializeField] private AnimationCurve emergenceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private Sprite natureSprite;
    
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D damageCollider;
    private Animator animator;
    private bool hasExploded = false;
    private float baseScale;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        damageCollider = GetComponent<CircleCollider2D>();
        animator = GetComponent<Animator>();
        
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
        baseScale = radius * 2f * sizeMultiplier;
        
        // Empezar invisible y pequeño para efecto de brote
        transform.localScale = Vector3.zero;
        if (spriteRenderer != null)
        {
            if (natureSprite != null) spriteRenderer.sprite = natureSprite;
            spriteRenderer.color = new Color(1, 1, 1, 0);
        }
        
        // Ajustar radio del collider
        if (damageCollider != null)
        {
            damageCollider.radius = 0.5f;
        }
        
        StartCoroutine(NatureWarningSequence());
    }

    private IEnumerator NatureWarningSequence()
    {
        float elapsed = 0f;
        Vector3 originalPos = transform.position;

        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / warningDuration;

            // 1. EMERGENCIA: Brotar del suelo
            float currentScale = emergenceCurve.Evaluate(progress) * baseScale;
            transform.localScale = Vector3.one * currentScale;

            if (spriteRenderer != null)
            {
                // 2. COLOR Y OPACIDAD:
                // Usamos un parpadeo sutil de opacidad solo durante el aviso
                float blink = 1f;
                if (progress < 0.8f) 
                {
                    blink = Mathf.Lerp(0.4f, 1f, Mathf.PingPong(Time.time * 12f, 1f));
                }

                // El color final es Blanco puro (sin tinte) para mostrar el sprite original
                Color finalColor = Color.white;
                finalColor.a = progress * blink;
                
                spriteRenderer.color = finalColor;

                // 3. TEMBLOR: La tierra vibra
                if (progress > 0.4f)
                {
                    float currentShake = shakeIntensity * (progress - 0.4f);
                    transform.position = originalPos + (Vector3)Random.insideUnitCircle * currentShake;
                }
            }

            yield return null;
        }

        // Asegurar estado final perfecto antes de explotar
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        transform.position = originalPos;
        
        Explode();
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // ACTIVAR ANIMACIÓN: Ejecuta el trigger de estallido si existe
        if (animator != null)
        {
            animator.SetTrigger("Burst");
        }

        // ACTIVAR DAÑO
        if (damageCollider != null)
        {
            damageCollider.enabled = true;
        }

        StartCoroutine(CleanupAfterAnimation());
    }

    private IEnumerator CleanupAfterAnimation()
    {
        // Esperamos a que la animación de estallido termine (ajustable)
        yield return new WaitForSeconds(0.6f);
        
        // Desvanecimiento suave final
        float fade = 1f;
        while (fade > 0)
        {
            fade -= Time.deltaTime * 2.5f;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = fade;
                spriteRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasExploded) return;
        
        if (collision.CompareTag("Player"))
        {
            Debug.Log("💥 Player atrapado por las raíces!");
            // Aquí se llamaría al método del Player para recibir daño
        }
    }
}

