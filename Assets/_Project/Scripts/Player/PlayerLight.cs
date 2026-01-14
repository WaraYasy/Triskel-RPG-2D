using UnityEngine;
using UnityEngine.Rendering.Universal;
using Triskel.Core;

/// <summary>
/// PlayerLight - Sistema de luz progresiva del jugador
/// Versión 2.0 - Tres niveles: Base → Con Lirio → Habilidad Activa
/// </summary>
[RequireComponent(typeof(Light2D))]
public class PlayerLight : MonoBehaviour
{
    [Header("Configuración de Intensidad")]
    [SerializeField] private float baseLightIntensity = 0.5f;        // Sin Lirio
    [SerializeField] private float lilioLightIntensity = 1.5f;       // Con Lirio recogido
    [SerializeField] private float activeAbilityIntensity = 3.0f;    // Habilidad activa (Z)
    
    [Header("Radio de Luz")]
    [SerializeField] private float baseRadius = 3f;                  // Radio sin Lirio
    [SerializeField] private float lilioRadius = 4f;                 // Radio con Lirio
    [SerializeField] private float activeAbilityRadius = 7f;         // Radio con habilidad
    
    [Header("Transición")]
    [SerializeField] private float transitionSpeed = 2f;
    
    private Light2D playerLight;
    private float targetIntensity;
    private float targetRadius;
    private bool hasLirio = false;
    private bool isAbilityActive = false;

    private void Awake()
    {
        playerLight = GetComponent<Light2D>();
        targetIntensity = baseLightIntensity;
        targetRadius = baseRadius;
        
        // Configurar luz inicial
        if (playerLight != null)
        {
            playerLight.intensity = baseLightIntensity;
            playerLight.pointLightOuterRadius = baseRadius;
        }
    }

    private void Start()
    {
        // Suscribirse al evento de inventario
        if (InventoryData.Instance != null)
        {
            InventoryData.Instance.OnItemAdded += OnItemCollected;
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar errores
        if (InventoryData.Instance != null)
        {
            InventoryData.Instance.OnItemAdded -= OnItemCollected;
        }
    }

    private void Update()
    {
        // Suavizar transición de intensidad
        if (playerLight != null)
        {
            if (Mathf.Abs(playerLight.intensity - targetIntensity) > 0.01f)
            {
                playerLight.intensity = Mathf.Lerp(
                    playerLight.intensity, 
                    targetIntensity, 
                    Time.deltaTime * transitionSpeed
                );
            }
            
            // Suavizar transición de radio
            if (Mathf.Abs(playerLight.pointLightOuterRadius - targetRadius) > 0.01f)
            {
                playerLight.pointLightOuterRadius = Mathf.Lerp(
                    playerLight.pointLightOuterRadius,
                    targetRadius,
                    Time.deltaTime * transitionSpeed
                );
            }
        }
    }

    private void OnItemCollected(CollectibleItem item)
    {
        // Verificar si es el Lirio
        if (item != null && item.itemID == "lirio")
        {
            hasLirio = true;
            
            // Solo aumentar si no hay habilidad activa
            if (!isAbilityActive)
            {
                targetIntensity = lilioLightIntensity;
                targetRadius = lilioRadius;
            }
            
            Debug.Log("💡 Lirio recogido - Luz mejorada!");
        }
    }

    /// <summary>
    /// Activa/desactiva la habilidad del Lirio (intensidad máxima)
    /// Llamar desde RelicSystem cuando se presiona Z
    /// </summary>
    public void ToggleLirioAbility()
    {
        if (!hasLirio)
        {
            Debug.LogWarning("⚠️ No tienes el Lirio todavía!");
            return;
        }
        
        isAbilityActive = !isAbilityActive;
        
        if (isAbilityActive)
        {
            // Luz MÁXIMA - Atrae fantasmas
            targetIntensity = activeAbilityIntensity;
            targetRadius = activeAbilityRadius;
            Debug.Log("🌟 Habilidad Lirio ACTIVADA - Luz máxima!");
        }
        else
        {
            // Volver a luz normal con Lirio
            targetIntensity = lilioLightIntensity;
            targetRadius = lilioRadius;
            Debug.Log("💡 Habilidad desactivada - Luz normal");
        }
    }

    // Getters públicos
    public bool HasLirio() => hasLirio;
    public bool IsAbilityActive() => isAbilityActive;
    public float GetCurrentIntensity() => playerLight != null ? playerLight.intensity : 0f;
}
