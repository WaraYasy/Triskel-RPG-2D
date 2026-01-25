using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// PlayerHealth - Gestiona la vida del jugador y el daño recibido.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 3f;
    private float currentHealth;

    [Header("Feedback de Daño")]
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float knockbackForce = 10f;

    [Header("Eventos")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnPlayerDeath;

    private Rigidbody2D rb;
    private bool isDead = false;
    private bool isInvulnerable = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (playerSprite == null) playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        // Notificar vida inicial
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    /// <summary>
    /// Recibe daño y activa efectos visuales/físicos.
    /// </summary>
    public void TakeDamage(float amount, Vector2 damageSourcePosition)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"[PlayerHealth] Vida actual: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HitFeedback(damageSourcePosition));
        }
    }

    private IEnumerator HitFeedback(Vector2 sourcePos)
    {
        isInvulnerable = true;

        // 1. Empujón (Knockback)
        if (rb != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - sourcePos).normalized;
            rb.linearVelocity = Vector2.zero; // Limpiar velocidad actual
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        // 2. Flash Rojo
        if (playerSprite != null) playerSprite.color = hitColor;
        
        // 3. Parpadeo e Invulnerabilidad
        float timer = 0;
        bool visible = true;
        
        while (timer < invulnerabilityDuration)
        {
            if (timer > 0.1f && playerSprite != null && playerSprite.color == hitColor)
                playerSprite.color = Color.white; // Quitar color rojo rápido

            if (playerSprite != null)
            {
                visible = !visible;
                playerSprite.enabled = visible;
            }

            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        if (playerSprite != null)
        {
            playerSprite.enabled = true;
            playerSprite.color = Color.white;
        }
        
        isInvulnerable = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[PlayerHealth] El jugador ha muerto.");
        if (playerSprite != null) playerSprite.color = Color.gray;

        // Notificar evento (para listeners en el Inspector)
        OnPlayerDeath?.Invoke();

        // Llamar al GameManager para transición de muerte
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnJugadorMuerto();
        }
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
