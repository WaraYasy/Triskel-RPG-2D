using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelLightingController - Gestiona la intensidad de la luz global por escena.
/// </summary>
public class LevelLightingController : MonoBehaviour
{
    [Header("Escenas y Modos")]
    [SerializeField] private string[] darknessScenes = { "Cueva", "Mina", "Cuadrante3" };
    [SerializeField] private string[] subtleScenes = { "Cuadrante1", "Cuadrante2", "Fortaleza" };

    [Header("Intensidades")]
    [Range(0, 1)] [SerializeField] private float darknessLevel = 0.05f; // Muy oscuro (Casi negro)
    [Range(0, 1)] [SerializeField] private float subtleLevel = 0.8f;   // Sutil (nublado)
    [Range(0, 1)] [SerializeField] private float normalLevel = 1.0f;   // Soleado

    [Header("Referencias")]
    [SerializeField] private Light2D playerLight; // Para excluirla del oscurecimiento

    private void Start()
    {
        ApplyLighting();
    }

    private void ApplyLighting()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        float targetIntensity = normalLevel;

        if (System.Array.Exists(darknessScenes, s => s == currentScene))
            targetIntensity = darknessLevel;
        else if (System.Array.Exists(subtleScenes, s => s == currentScene))
            targetIntensity = subtleLevel;

        Light2D[] allLights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        foreach (Light2D light in allLights)
        {
            // Solo afectamos a las luces GLOBALES del nivel
            if (light.lightType == Light2D.LightType.Global && light != playerLight)
            {
                light.intensity = targetIntensity;
            }
        }

        Debug.Log($"[Lighting] Escena '{currentScene}' configurada con intensidad: {targetIntensity}");
    }
}
