using UnityEngine;
using System.Collections;
using Triskel.API;

/// <summary>
/// TruthAltar - Altar de la Verdad donde el Manto de Luna revela la verdad oculta.
/// Cuando el jugador usa el Manto cerca del altar:
/// 1. El nivel se ilumina
/// 2. Las sombras revelan su forma verdadera (animales asustados)
/// 3. El jugador puede decidir si evitarlos o ayudarlos
/// </summary>
public class TruthAltar : MonoBehaviour
{
    [Header("Configuración del Altar")]
    [SerializeField] private float activationRadius = 2f;
    [Tooltip("El jugador debe tener el Manto activo y estar dentro del radio")]
    
    [Header("Efecto de Iluminación (Amanecer)")]
    [SerializeField] private float revealedLightIntensity = 1.0f;
    [SerializeField] private Color revealedLightColor = new Color(1f, 0.95f, 0.8f); // Luz cálida de sol
    [SerializeField] private float lightTransitionDuration = 5f; // Más lento para efecto amanecer
    
    [Header("Sombras a Revelar")]
    [Tooltip("Arrastra aquí todas las sombras que serán reveladas como animales")]
    [SerializeField] private ShadowPatrol[] shadowsToReveal;
    
    [Header("Efectos Visuales")]
    [SerializeField] private GameObject altarGlow;           // Brillo del altar
    [SerializeField] private ParticleSystem activationParticles;
    [SerializeField] private SpriteRenderer altarSprite;
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.5f);
    [SerializeField] private Color activeColor = new Color(0.8f, 0.9f, 1f);
    
    [Header("Sprites (Luna/Sol)")]
    [SerializeField] private Sprite moonSprite;              // Sprite estático de luna
    [SerializeField] private Sprite sunSprite;               // Sprite estático de sol
    [SerializeField] private float transitionAnimDuration = 2f; // Duración de tu animación de transición
    
    [Header("Animación")]
    [SerializeField] private Animator altarAnimator;
    [SerializeField] private string activateTriggerName = "Activate";
    
    [Header("Audio")]
    [SerializeField] private AudioClip revelationSound;
    
    // Estado
    private bool hasBeenActivated = false;
    private bool playerInRange = false;
    private Transform playerTransform;
    private RelicSystem relicSystem;
    private AudioSource audioSource;
    
    // Sistema de iluminación
    private static bool truthRevealed = false;
    public static bool IsTruthRevealed => truthRevealed;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Buscar Animator si no está asignado
        if (altarAnimator == null)
        {
            altarAnimator = GetComponent<Animator>();
        }
        
        // IMPORTANTE: Desactivar Animator al inicio para que no reproduzca la animación
        if (altarAnimator != null)
        {
            altarAnimator.enabled = false;
        }
        
        if (altarSprite != null)
        {
            altarSprite.color = inactiveColor;
            
            // Establecer sprite inicial de luna
            if (moonSprite != null)
            {
                altarSprite.sprite = moonSprite;
            }
        }
        if (altarGlow != null)
        {
            altarGlow.SetActive(false);
        }
    }
    
    private void Start()
    {
        FindPlayer();
        
        // Si ya se reveló la verdad en esta sesión, mantener el estado
        if (truthRevealed)
        {
            ApplyRevealedState();
        }
    }
    
    private void Update()
    {
        if (hasBeenActivated) return;
        if (playerTransform == null) FindPlayer();
        if (relicSystem == null) return;
        
        // Comprobar si el jugador está en rango con el Manto activo
        float distance = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= activationRadius;
        
        if (playerInRange && relicSystem.IsInvisible())
        {
            ActivateAltar();
        }
        
        // Efecto visual de proximidad (el altar brilla ligeramente cuando estás cerca con el manto)
        if (playerInRange && relicSystem.GetCurrentRelic() == RelicSystem.RelicType.MantoDeLuna)
        {
            if (altarGlow != null && !altarGlow.activeSelf)
            {
                altarGlow.SetActive(true);
            }
        }
        else if (altarGlow != null && altarGlow.activeSelf && !hasBeenActivated)
        {
            altarGlow.SetActive(false);
        }
    }
    
    private void ActivateAltar()
    {
        if (hasBeenActivated) return;
        hasBeenActivated = true;
        truthRevealed = true;
        
        Debug.Log("🌙✨ ¡ALTAR DE LA VERDAD ACTIVADO! La luz revela la verdad oculta...");
        
        // Modificar moral y registrar decisión (Buena decisión: Revelar la verdad)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ModifyMoralWithChoice(1, APIConstants.Choices.REVELAR);
            Debug.Log("[TruthAltar] +1 Moral (Verdad Revelada)");
        }

        // Reproducir sonido
        if (audioSource != null && revelationSound != null)
        {
            audioSource.PlayOneShot(revelationSound);
        }
        
        // Activar animación de transición Luna -> Sol
        if (altarAnimator != null)
        {
            // Activar el Animator (estaba desactivado para evitar autoplay)
            altarAnimator.enabled = true;
            altarAnimator.SetTrigger(activateTriggerName);
            // Cambiar a sprite del sol después de la animación
            StartCoroutine(ChangeToSunSprite());
        }
        else if (sunSprite != null && altarSprite != null)
        {
            // Si no hay animator, cambiar sprite directamente
            altarSprite.sprite = sunSprite;
        }
        
        // Activar partículas
        if (activationParticles != null)
        {
            activationParticles.Play();
        }
        
        // Cambiar color del altar (solo si no hay animator)
        if (altarSprite != null && altarAnimator == null)
        {
            StartCoroutine(TransitionAltarColor());
        }
        
        // Iluminar el nivel
        StartCoroutine(RevealTruth());
    }
    
    /// <summary>
    /// Cambia al sprite del sol después de que termine la animación de transición.
    /// </summary>
    private IEnumerator ChangeToSunSprite()
    {
        // Esperar a que termine la animación de transición
        yield return new WaitForSeconds(transitionAnimDuration);
        
        // Desactivar el animator para que no interfiera con el sprite
        if (altarAnimator != null)
        {
            altarAnimator.enabled = false;
        }
        
        // Cambiar al sprite del sol
        if (altarSprite != null && sunSprite != null)
        {
            altarSprite.sprite = sunSprite;
            altarSprite.color = activeColor;
            Debug.Log("☀️ Altar transformado al estado de Sol");
        }
    }
    
    private IEnumerator RevealTruth()
    {
        // 1. Transición de iluminación
        yield return StartCoroutine(TransitionLighting());
        
        // 2. Revelar la verdadera forma de las sombras
        RevealShadows();
        
        Debug.Log("🐾 Las sombras han revelado su verdadera forma: ¡Son animales asustados!");
    }
    
    private IEnumerator TransitionLighting()
    {
        // Buscar todas las luces globales
        var globalLights = FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None);
        
        float elapsed = 0f;
        float[] startIntensities = new float[globalLights.Length];
        Color[] startColors = new Color[globalLights.Length];
        
        // Guardar intensidades y colores iniciales
        for (int i = 0; i < globalLights.Length; i++)
        {
            if (globalLights[i].lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
            {
                startIntensities[i] = globalLights[i].intensity;
                startColors[i] = globalLights[i].color;
            }
        }
        
        while (elapsed < lightTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightTransitionDuration;
            // Usar curva suave para la intensidad (SmoothStep)
            float smoothT = t * t * (3f - 2f * t); 
            
            for (int i = 0; i < globalLights.Length; i++)
            {
                if (globalLights[i].lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
                {
                    // Interpolar intensidad
                    globalLights[i].intensity = Mathf.Lerp(startIntensities[i], revealedLightIntensity, smoothT);
                    
                    // Interpolar color (Efecto amanecer: de azul oscuro a cálido)
                    globalLights[i].color = Color.Lerp(startColors[i], revealedLightColor, smoothT);
                }
            }
            
            yield return null;
        }
        
        // Asegurar valores finales
        for (int i = 0; i < globalLights.Length; i++)
        {
            if (globalLights[i].lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
            {
                globalLights[i].intensity = revealedLightIntensity;
                globalLights[i].color = revealedLightColor;
            }
        }
    }
    
    private IEnumerator TransitionAltarColor()
    {
        float elapsed = 0f;
        Color startColor = altarSprite.color;
        
        while (elapsed < lightTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightTransitionDuration;
            altarSprite.color = Color.Lerp(startColor, activeColor, t);
            yield return null;
        }
    }
    
    private void RevealShadows()
    {
        // Revelar todas las sombras específicas
        if (shadowsToReveal != null)
        {
            foreach (var shadow in shadowsToReveal)
            {
                if (shadow != null)
                {
                    shadow.RevealTrueForm();
                }
            }
        }
        
        // También buscar todas las sombras en la escena (por si no están asignadas)
        var allShadows = FindObjectsByType<ShadowPatrol>(FindObjectsSortMode.None);
        foreach (var shadow in allShadows)
        {
            shadow.RevealTrueForm();
        }
    }
    
    private void ApplyRevealedState()
    {
        // Aplicar estado revelado inmediatamente (para cuando se recarga la escena)
        if (altarSprite != null)
        {
            altarSprite.color = activeColor;
        }
        if (altarGlow != null)
        {
            altarGlow.SetActive(true);
        }
        hasBeenActivated = true;
        
        // Iluminar nivel inmediatamente
        var globalLights = FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None);
        foreach (var light in globalLights)
        {
            if (light.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
            {
                light.intensity = revealedLightIntensity;
                light.color = revealedLightColor;
            }
        }
        
        // Revelar sombras
        RevealShadows();
    }
    
    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            relicSystem = player.GetComponent<RelicSystem>();
        }
    }
    
    /// <summary>
    /// Resetear el estado de revelación (para cuando se empiece una nueva partida)
    /// </summary>
    public static void ResetTruthState()
    {
        truthRevealed = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Radio de activación
        Gizmos.color = new Color(0.8f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, activationRadius);
        
        Gizmos.color = new Color(0.8f, 0.8f, 1f, 0.2f);
        Gizmos.DrawSphere(transform.position, activationRadius);
    }
}
