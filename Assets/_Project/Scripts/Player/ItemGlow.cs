using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Triskel.Player
{
    /// <summary>
    /// ItemGlow - Añade efecto de luz a items coleccionables importantes
    /// Se desactiva automáticamente al recoger el item
    /// </summary>
    [RequireComponent(typeof(CollectibleObject))]
    public class ItemGlow : MonoBehaviour
    {
        [Header("Configuración de Luz")]
        [SerializeField] private Color glowColor = new Color(0.5f, 0.8f, 1f); // Cyan
        [SerializeField] private float intensity = 1.2f;
        [SerializeField] private float radius = 3f;
        
        [Header("Pulsación (Opcional)")]
        [SerializeField] private bool pulseEffect = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.3f; // Cuánto varía la intensidad
        
        private Light2D itemLight;
        private float baseIntensity;
        private CollectibleObject collectible;

        private void Awake()
        {
            collectible = GetComponent<CollectibleObject>();
            CreateLight();
        }

        private void CreateLight()
        {
            // Crear Light 2D si no existe
            itemLight = GetComponent<Light2D>();
            
            if (itemLight == null)
            {
                itemLight = gameObject.AddComponent<Light2D>();
            }
            
            // Configurar luz
            itemLight.lightType = Light2D.LightType.Point;
            itemLight.color = glowColor;
            itemLight.intensity = intensity;
            itemLight.pointLightOuterRadius = radius;
            itemLight.pointLightInnerRadius = 0;
            
            baseIntensity = intensity;
        }

        private void Update()
        {
            if (itemLight == null) return;
            
            // Efecto de pulsación
            if (pulseEffect)
            {
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                itemLight.intensity = baseIntensity + (pulse * pulseAmount);
            }
        }

        private void OnDisable()
        {
            // Desactivar luz cuando se recoge el item
            if (itemLight != null)
            {
                itemLight.enabled = false;
            }
        }
    }
}
