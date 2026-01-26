using UnityEngine;
using UnityEngine.Rendering.Universal;
using Triskel.Core;

/// <summary>
/// PlayerLight - Controla el halo de luz de Sacha y el Lirio.
/// </summary>
[RequireComponent(typeof(Light2D))]
public class PlayerLight : MonoBehaviour
{
    [Header("Intensidad del Halo")]
    [SerializeField] private float baseIntensity = 0.2f;
    [SerializeField] private float lilioIntensity = 0.8f;
    [SerializeField] private float abilityIntensity = 1.5f;

    [Header("Radio del Halo")]
    [SerializeField] private float baseRadius = 2f;
    [SerializeField] private float lilioRadius = 4f;
    [SerializeField] private float abilityRadius = 6f;

    [Header("Ajustes")]
    [SerializeField] private float transitionSpeed = 3f;

    private Light2D playerLight;
    private float targetIntensity;
    private float targetRadius;
    private bool hasLirio = false;
    private bool isAbilityActive = false;

    private void Awake()
    {
        playerLight = GetComponent<Light2D>();
        targetIntensity = baseIntensity;
        targetRadius = baseRadius;
    }

    private void Start()
    {
        if (InventoryData.Instance != null)
        {
            InventoryData.Instance.OnItemAdded += OnItemCollected;
            
            // Comprobar si ya tenemos el Lirio al empezar el nivel (persistencia)
            if (InventoryData.Instance.HasItem("lirio"))
            {
                hasLirio = true;
                targetIntensity = lilioIntensity;
                targetRadius = lilioRadius;
                Debug.Log("[PlayerLight] Lirio detectado en inventario al inicio.");
            }
        }
    }

    private void OnDestroy()
    {
        if (InventoryData.Instance != null)
            InventoryData.Instance.OnItemAdded -= OnItemCollected;
    }

    private void Update()
    {
        if (playerLight == null) return;

        playerLight.intensity = Mathf.Lerp(playerLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
        playerLight.pointLightOuterRadius = Mathf.Lerp(playerLight.pointLightOuterRadius, targetRadius, Time.deltaTime * transitionSpeed);
    }

    private void OnItemCollected(CollectibleItem item)
    {
        if (item != null && item.itemID == "lirio")
        {
            hasLirio = true;
            if (!isAbilityActive)
            {
                targetIntensity = lilioIntensity;
                targetRadius = lilioRadius;
            }
        }
    }

    public void ToggleLirioAbility()
    {
        if (!hasLirio) return;
        isAbilityActive = !isAbilityActive;
        
        targetIntensity = isAbilityActive ? abilityIntensity : lilioIntensity;
        targetRadius = isAbilityActive ? abilityRadius : lilioRadius;
    }

    public bool IsAbilityActive() => isAbilityActive;
    public bool HasLirio() => hasLirio;
}
