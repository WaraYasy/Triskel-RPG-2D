using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Sistema de Reliquias - Cambio y uso de habilidades
/// Versión 3.0 - REFACTORIZADO para Input System
/// </summary>
public class RelicSystem : MonoBehaviour
{
    public enum RelicType
    {
        None = 0,
        LirioAzul = 1,
        HachaSagrada = 2,
        MantoDeLuna = 3
    }

    [Header("Configuración")]
    [SerializeField] private RelicType currentRelic = RelicType.None;
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer relicIndicator;
    [SerializeField] private Color colorLirio = Color.cyan;
    [SerializeField] private Color colorHacha = Color.red;
    [SerializeField] private Color colorManto = new Color(0.5f, 0f, 0.5f);
    
    [Header("Habilidades")]
    [SerializeField] private float abilityCooldown = 2f;
    [SerializeField] private SpriteRenderer playerSprite; // Para invisibilidad del Manto
    [SerializeField] private GameObject lilioLightPrefab; // Prefab de luz del Lirio
    
    private float abilityCooldownTimer = 0f;
    private bool isInvisible = false;
    private GameObject activeLilioLight; // Referencia a la luz activa
    private bool isLilioActive = false;
    private PlayerController playerController; // Referencia para dirección
    private PlayerInputActions inputActions;
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        inputActions = new PlayerInputActions();
    }
    
    private void OnEnable()
    {
        inputActions.Enable();
        
        // Suscribirse a eventos de selección DIRECTA (PC - Teclas 1/2/3)
        inputActions.Player.SelectRelic1.performed += ctx => SelectRelic(RelicType.LirioAzul);
        inputActions.Player.SelectRelic2.performed += ctx => SelectRelic(RelicType.HachaSagrada);
        inputActions.Player.SelectRelic3.performed += ctx => SelectRelic(RelicType.MantoDeLuna);
        
        // Suscribirse a evento de CICLAR reliquia (Móvil - Botón Next)
        inputActions.Player.CycleRelic.performed += OnCycleRelicPerformed;
        
        // Suscribirse a evento de usar reliquia
        inputActions.Player.UseRelic.performed += OnUseRelicPerformed;
    }

    private void Start()
    {
        // Comprobar si ya tenemos el Lirio en el inventario al empezar el nivel
        if (Triskel.Core.InventoryData.Instance != null && Triskel.Core.InventoryData.Instance.HasItem("lirio"))
        {
            SelectRelic(RelicType.LirioAzul);
            Debug.Log("[RelicSystem] Lirio detectado en inventario. Equipado automáticamente.");
        }
    }
    
    private void OnDisable()
    {
        // Desuscribirse de eventos
        inputActions.Player.SelectRelic1.performed -= ctx => SelectRelic(RelicType.LirioAzul);
        inputActions.Player.SelectRelic2.performed -= ctx => SelectRelic(RelicType.HachaSagrada);
        inputActions.Player.SelectRelic3.performed -= ctx => SelectRelic(RelicType.MantoDeLuna);
        inputActions.Player.CycleRelic.performed -= OnCycleRelicPerformed;
        inputActions.Player.UseRelic.performed -= OnUseRelicPerformed;
        
        inputActions.Disable();
    }
    
    private void Update()
    {
        // Cooldown de habilidad
        if (abilityCooldownTimer > 0)
        {
            abilityCooldownTimer -= Time.deltaTime;
        }
        
        // Actualizar posición de la luz del Lirio si está activa
        if (isLilioActive && activeLilioLight != null)
        {
            activeLilioLight.transform.position = transform.position;
        }
    }
    
    // Callback del Input System para ciclar reliquia
    private void OnCycleRelicPerformed(InputAction.CallbackContext context)
    {
        CycleToNextRelic();
    }
    
    // Callback del Input System para usar reliquia
    private void OnUseRelicPerformed(InputAction.CallbackContext context)
    {
        if (abilityCooldownTimer <= 0)
        {
            UseCurrentRelic();
        }
    }
    
    /// <summary>
    /// Cicla a la siguiente reliquia (para móviles)
    /// </summary>
    public void CycleToNextRelic()
    {
        // Ciclar: Lirio → Hacha → Manto → Lirio
        int nextIndex = ((int)currentRelic % 3) + 1;
        currentRelic = (RelicType)nextIndex;
        
        SelectRelic(currentRelic);
        Debug.Log($"🔄 Reliquia ciclada a: {currentRelic}");
    }
    
    public void SelectRelic(RelicType relic)
    {
        currentRelic = relic;
        UpdateVisualFeedback();
        Debug.Log($"Reliquia seleccionada: {relic}");
    }
    
    private void UpdateVisualFeedback()
    {
        if (relicIndicator == null) return;
        
        switch (currentRelic)
        {
            case RelicType.None:
                relicIndicator.color = Color.white;
                relicIndicator.enabled = false;
                break;
            
            case RelicType.LirioAzul:
                relicIndicator.color = colorLirio;
                relicIndicator.enabled = true;
                break;
            
            case RelicType.HachaSagrada:
                relicIndicator.color = colorHacha;
                relicIndicator.enabled = true;
                break;
            
            case RelicType.MantoDeLuna:
                relicIndicator.color = colorManto;
                relicIndicator.enabled = true;
                break;
        }
    }
    
    private void UseCurrentRelic()
    {
        if (currentRelic == RelicType.None)
        {
            Debug.LogWarning("No hay reliquia equipada");
            return;
        }
        
        // Activar cooldown
        abilityCooldownTimer = abilityCooldown;
        
        // Ejecutar habilidad según reliquia
        switch (currentRelic)
        {
            case RelicType.LirioAzul:
                UseLirio();
                break;
            
            case RelicType.HachaSagrada:
                UseHacha();
                break;
            
            case RelicType.MantoDeLuna:
                UseManto();
                break;
        }
    }
    
    // === HABILIDADES ===
    
    private void UseLirio()
    {
        // Delegar a PlayerLight para manejar la intensidad
        PlayerLight playerLight = GetComponent<PlayerLight>();
        
        if (playerLight != null)
        {
            playerLight.ToggleLirioAbility();
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró PlayerLight en el Player!");
        }
    }
    
    [Header("Hacha Settings")]
    [SerializeField] private float axeRange = 1.2f;
    [SerializeField] private float axeRadius = 0.6f;

    private void UseHacha()
    {
        Debug.Log("⚔️ Hacha Sagrada - Golpe");
        
        // Obtener dirección del movimiento
        Vector2 direction = playerController != null ? 
            playerController.GetLastMoveDirection() : Vector2.down;
        
        // Posición del golpe (frente al jugador)
        Vector2 hitPosition = (Vector2)transform.position + direction * axeRange;

        // Detectar impactos en 2D
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPosition, axeRadius);
        
        bool hitAnything = false;
        foreach (var hit in hits)
        {
            // 1. Cristales Malignos (Cuadrante 2 - Fortaleza del Gigante)
            var crystal = hit.GetComponent<Triskel.GiantFortress.MalignCrystal>();
            if (crystal != null)
            {
                crystal.OnAxeHit();
                hitAnything = true;
                continue;
            }

            // 2. Árboles Malditos (Cuadrante 2 - Fortaleza del Gigante)
            var tree = hit.GetComponent<Triskel.GiantFortress.CursedTree>();
            if (tree != null)
            {
                tree.OnAxeHit();
                hitAnything = true;
                continue;
            }

            // 3. Otros objetos destructibles (futuro)
            var destructible = hit.GetComponent<Triskel.GiantFortress.IAxeDestructible>();
            if (destructible != null)
            {
                destructible.OnAxeHit();
                hitAnything = true;
            }
        }

        if (hitAnything)
        {
            Debug.Log("🎯 ¡Impacto!");
        }
    }
    
    private void UseManto()
    {
        isInvisible = !isInvisible;
        
        if (playerSprite != null)
        {
            Color color = playerSprite.color;
            color.a = isInvisible ? 0.3f : 1f;
            playerSprite.color = color;
        }
        
        Debug.Log($"🌙 Manto de Luna - Invisibilidad: {(isInvisible ? "ON" : "OFF")}");
    }
    
    // Métodos públicos
    public RelicType GetCurrentRelic() => currentRelic;
    public bool HasRelicEquipped() => currentRelic != RelicType.None;
    public float GetAbilityCooldownProgress() => 1f - (abilityCooldownTimer / abilityCooldown);
    public bool IsInvisible() => isInvisible;
}
