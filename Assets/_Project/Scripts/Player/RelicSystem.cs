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
    
    private void UseHacha()
    {
        Debug.Log("⚔️ Hacha Sagrada - Golpe direccional");
        
        // Obtener dirección del movimiento
        Vector2 direction = playerController != null ? 
            playerController.GetLastMoveDirection() : Vector2.down;
        
        // Crear proyectil visual simple
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Cube);
        projectile.transform.position = transform.position;
        projectile.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        var renderer = projectile.GetComponent<Renderer>();
        renderer.material.color = colorHacha;
        
        // Mover en dirección
        var rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = (Vector3)direction * 10f;
        
        // Destruir después de 2 segundos
        Destroy(projectile, 2f);
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

