using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// PlayerStealth - Sistema de sigilo del jugador.
/// Gestiona el estado de ocultación y ruido del jugador.
/// Mecánica de sigilo para el Cuadrante 3.
/// </summary>
public class PlayerStealth : MonoBehaviour
{
    [Header("Configuración de Ruido")]
    [SerializeField] private float noiseRadius = 3f;           // Radio al que las sombras pueden oír
    [SerializeField] private float dashNoiseMultiplier = 2f;   // El dash hace más ruido
    [SerializeField] private float noiseDuration = 0.5f;       // Duración del "ruido" después de una acción
    
    [Header("Velocidad Sigilosa")]
    [SerializeField] private float stealthSpeedMultiplier = 0.5f;  // Al agacharse se mueve más lento
    
    [Header("UI")]
    [SerializeField] private GameObject stealthUI;              // UI de sigilo activo
    [SerializeField] private UnityEvent OnHidden;
    [SerializeField] private UnityEvent OnRevealed;
    [SerializeField] private UnityEvent OnNoiseAlert;
    
    // Estado
    private bool isHidden = false;
    private bool isCrouching = false;
    private bool isMakingNoise = false;
    private float noiseTimer = 0f;
    private SafeZone currentSafeZone;
    
    // Referencias
    private PlayerController playerController;
    private Rigidbody2D rb;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Manejar timer de ruido
        if (isMakingNoise)
        {
            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0)
            {
                isMakingNoise = false;
            }
        }
        
        // Detectar acciones ruidosas
        DetectNoisyActions();
    }

    /// <summary>
    /// Detecta acciones que producen ruido (dash, correr, etc.)
    /// </summary>
    private void DetectNoisyActions()
    {
        if (isHidden) return;
        
        // Detectar dash
        if (playerController != null && playerController.IsDashing())
        {
            MakeNoise(noiseRadius * dashNoiseMultiplier);
        }
    }

    /// <summary>
    /// Genera ruido que puede alertar a las sombras cercanas.
    /// </summary>
    public void MakeNoise(float radius)
    {
        if (isHidden) return;
        
        isMakingNoise = true;
        noiseTimer = noiseDuration;
        
        Debug.Log($"🔊 Jugador hace ruido en radio {radius}");
        
        // Alertar a todas las sombras en el radio
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hitCollider in hitColliders)
        {
            ShadowPatrol shadow = hitCollider.GetComponent<ShadowPatrol>();
            if (shadow != null)
            {
                shadow.AlertToNoise(transform.position);
            }
        }
        
        OnNoiseAlert?.Invoke();
    }

    /// <summary>
    /// Establecer estado de ocultación (llamado por SafeZone).
    /// </summary>
    public void SetHidden(bool hidden, SafeZone spot)
    {
        bool wasHidden = isHidden;
        isHidden = hidden;
        currentSafeZone = spot;
        
        if (hidden && !wasHidden)
        {
            Debug.Log("👁 Jugador ahora está oculto");
            OnHidden?.Invoke();
            
            // Detener movimiento al esconderse
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else if (!hidden && wasHidden)
        {
            Debug.Log("👁 Jugador ahora es visible");
            OnRevealed?.Invoke();
        }
        
        // Actualizar UI
        if (stealthUI != null)
        {
            stealthUI.SetActive(hidden);
        }
    }

    /// <summary>
    /// Toggle de modo agacharse para moverse sigilosamente.
    /// </summary>
    public void ToggleCrouch()
    {
        if (isHidden) return;
        
        isCrouching = !isCrouching;
        Debug.Log(isCrouching ? "🦶 Modo sigiloso activado" : "🏃 Modo normal");
    }

    /// <summary>
    /// Fuerza al jugador a salir del escondite.
    /// </summary>
    public void ForceReveal()
    {
        if (isHidden && currentSafeZone != null)
        {
            SetHidden(false, null);
        }
    }

    #region Getters

    public bool IsHidden() => isHidden;
    public bool IsCrouching() => isCrouching;
    public bool IsMakingNoise() => isMakingNoise;
    public float GetStealthSpeedMultiplier() => isCrouching ? stealthSpeedMultiplier : 1f;
    public float GetNoiseRadius() => noiseRadius;

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        // Radio de ruido normal
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
        
        // Radio de ruido con dash
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, noiseRadius * dashNoiseMultiplier);
    }

    #endregion
}
