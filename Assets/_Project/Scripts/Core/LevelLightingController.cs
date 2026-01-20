using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelLightingController - Controla si el sistema de luz está activo según el nivel
/// Versión 1.0 - Solo activa luz en niveles específicos
/// </summary>
public class LevelLightingController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombres de las escenas donde la luz DEBE estar activa")]
    [SerializeField] private string[] scenesWithDarkness = { "Cuadrante1", "Nivel1" };
    
    [Header("Referencias")]
    [SerializeField] private Light2D playerLight;
    
    [Header("Configuración de Iluminación")]
    [Tooltip("Intensidad de luz en niveles oscuros")]
    [SerializeField] private float darknessIntensity = 0.2f;
    [Tooltip("Intensidad de luz en niveles normales")]
    [SerializeField] private float normalIntensity = 1.0f;
    
    private Light2D[] sceneLights; // Todas las luces de la escena

    private void Start()
    {
        // Buscar todas las luces globales en la escena
        sceneLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        
        // Detectar escena actual
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Verificar si esta escena debe tener oscuridad
        bool shouldHaveDarkness = System.Array.Exists(scenesWithDarkness, scene => scene == currentScene);
        
        if (shouldHaveDarkness)
        {
            ActivateDarknessSystem();
        }
        else
        {
            DeactivateDarknessSystem();
        }
        
        Debug.Log($"[LevelLighting] Escena: {currentScene} | Oscuridad: {shouldHaveDarkness}");
    }
    
    private void ActivateDarknessSystem()
    {
        // Activar luz del player
        if (playerLight != null)
        {
            playerLight.enabled = true;
        }
        
        // OSCURECER todas las luces globales (excepto la del player)
        foreach (Light2D light in sceneLights)
        {
            if (light != playerLight && light.lightType == Light2D.LightType.Global)
            {
                light.intensity = darknessIntensity;
                Debug.Log($"[LevelLighting] Luz global oscurecida: {light.name}");
            }
        }
    }
    
    private void DeactivateDarknessSystem()
    {
        // Desactivar luz del player (no es necesaria)
        if (playerLight != null)
        {
            playerLight.enabled = false;
        }
        
        // ILUMINAR todas las luces globales
        foreach (Light2D light in sceneLights)
        {
            if (light != playerLight && light.lightType == Light2D.LightType.Global)
            {
                light.intensity = normalIntensity;
                Debug.Log($"[LevelLighting] Luz global restaurada: {light.name}");
            }
        }
    }
}
