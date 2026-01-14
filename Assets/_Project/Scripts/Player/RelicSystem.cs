using UnityEngine;

/// <summary>
/// Sistema de Reliquias - Cambio y uso de habilidades
/// Versión 2.0 - Con habilidades básicas por reliquia
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
    [SerializeField] private float lilioDetectionRadius = 3f;
    [SerializeField] private SpriteRenderer playerSprite; // Para invisibilidad del Manto
    [SerializeField] private GameObject lilioLightPrefab; // Prefab de luz del Lirio
    
    private float abilityCooldownTimer = 0f;
    private bool isInvisible = false;
    private GameObject activeLilioLight; // Referencia a la luz activa
    private bool isLilioActive = false;
    private PlayerController playerController; // Referencia para dirección
    
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
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
        
        // Cambio de reliquia con teclas numéricas
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectRelic(RelicType.LirioAzul);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectRelic(RelicType.HachaSagrada);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectRelic(RelicType.MantoDeLuna);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            SelectRelic(RelicType.None);
        }
        
        // Usar habilidad con tecla Z
        if (Input.GetKeyDown(KeyCode.Z) && abilityCooldownTimer <= 0)
        {
            UseCurrentRelic();
        }
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
        // Toggle ON/OFF
        isLilioActive = !isLilioActive;
        
        if (isLilioActive)
        {
            Debug.Log("🌸 Lirio Azul ACTIVADO - Luz encendida");
            
            // Crear luz si no existe
            if (activeLilioLight == null)
            {
                if (lilioLightPrefab != null)
                {
                    // Usar prefab personalizado
                    activeLilioLight = Instantiate(lilioLightPrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    // Crear luz básica si no hay prefab
                    activeLilioLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    activeLilioLight.transform.localScale = Vector3.one * lilioDetectionRadius * 2f;
                    
                    var renderer = activeLilioLight.GetComponent<Renderer>();
                    renderer.material.color = new Color(0, 1, 1, 0.3f); // Cyan transparente
                    
                    // Eliminar collider (solo visual)
                    Destroy(activeLilioLight.GetComponent<Collider>());
                }
                
                // Tag para que los fantasmas lo encuentren
                activeLilioLight.tag = "LirioLight";
                activeLilioLight.name = "LirioLight";
            }
            
            activeLilioLight.SetActive(true);
        }
        else
        {
            Debug.Log("🌸 Lirio Azul DESACTIVADO - Luz apagada");
            
            // Desactivar luz
            if (activeLilioLight != null)
            {
                activeLilioLight.SetActive(false);
            }
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

