using System.Collections;
using UnityEngine;

/// <summary>
/// GhostLiberation - Maneja la mecánica de liberación de fantasmas
/// Versión 1.0 - Abrazo que libera al fantasma con efecto visual
/// </summary>
public class GhostLiberation : MonoBehaviour
{
    [Header("Configuración de Liberación")]
    [SerializeField] private float liberationDuration = 2.5f;    // Duración del efecto de liberación
    [SerializeField] private float ascendHeight = 4f;            // Altura que asciende
    
    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private Color liberationColor = Color.cyan; // Color del efecto
    [SerializeField] private bool useParticles = false;          // Si quieres partículas
    
    private bool isBeingLiberated = false;
    private GhostAI ghostAI;

    private void Awake()
    {
        ghostAI = GetComponent<GhostAI>();
    }

    /// <summary>
    /// Método llamado por la LiberationFountain para liberar al fantasma.
    /// </summary>
    public void LiberateAtFountain()
    {
        if (isBeingLiberated) return;
        
        isBeingLiberated = true;
        
        // Desactivar IA del fantasma
        if (ghostAI != null)
        {
            ghostAI.enabled = false;
        }

        // Efecto de liberación
        StartCoroutine(LiberationEffect());
        
        Debug.Log("👻 ¡Fantasma liberado en la fuente!");
    }


    private IEnumerator LiberationEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite == null)
        {
            Destroy(gameObject);
            yield break;
        }
        
        Color originalColor = sprite.color;
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        
        // Opcional: Crear partículas
        GameObject particles = null;
        if (useParticles)
        {
            particles = CreateLiberationParticles();
        }
        
        // Animación de ascenso y fade
        while (elapsed < liberationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / liberationDuration;
            
            // Fade out (desvanecerse)
            float alpha = Mathf.Lerp(1f, 0f, progress);
            sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            // Move up (ascender)
            transform.position = startPos + Vector3.up * progress * ascendHeight;
            
            // Opcional: Rotar suavemente
            transform.Rotate(Vector3.forward * Time.deltaTime * 30f);
            
            yield return null;
        }
        
        // Destruir fantasma
        Destroy(gameObject);
        
        // Limpiar partículas
        if (particles != null)
        {
            Destroy(particles, 2f);
        }
    }

    private GameObject CreateLiberationParticles()
    {
        GameObject particlesObj = new GameObject("LiberationParticles");
        particlesObj.transform.position = transform.position;
        
        ParticleSystem ps = particlesObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.startColor = liberationColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startLifetime = 2f;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = ps.emission;
        emission.rateOverTime = 40f;
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        return particlesObj;
    }

}
