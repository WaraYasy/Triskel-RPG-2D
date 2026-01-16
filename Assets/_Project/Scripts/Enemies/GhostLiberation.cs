using System.Collections;
using UnityEngine;

/// <summary>
/// GhostLiberation - Maneja la mecánica de liberación de fantasmas
/// Versión 1.0 - Abrazo que libera al fantasma con efecto visual
/// </summary>
public class GhostLiberation : MonoBehaviour
{
    [Header("Configuración de Liberación")]
    [SerializeField] private float embraceDistance = 0.8f;       // Distancia para "abrazar"
    [SerializeField] private float freezeDuration = 2f;          // Tiempo que congela al player
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

    private void Update()
    {
        if (isBeingLiberated) return;
        
        // Buscar player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        // Verificar distancia
        float distance = Vector2.Distance(transform.position, player.transform.position);
        
        if (distance <= embraceDistance)
        {
            TriggerLiberation(player);
        }
    }

    private void TriggerLiberation(GameObject player)
    {
        isBeingLiberated = true;
        
        // Desactivar IA del fantasma
        if (ghostAI != null)
        {
            ghostAI.enabled = false;
        }
        
        // Congelar player
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            StartCoroutine(FreezePlayer(playerController, freezeDuration));
        }
        
        // Incrementar moral
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ModifyMoral(1);
        }
        
        // Efecto de liberación
        StartCoroutine(LiberationEffect());
        
        Debug.Log("👻 ¡Fantasma liberado! +1 Moral");
    }

    private IEnumerator FreezePlayer(PlayerController player, float duration)
    {
        // Guardar estado original
        bool wasEnabled = player.enabled;
        
        // Desactivar control
        player.enabled = false;
        
        // Opcional: Efecto visual de freeze
        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        yield return new WaitForSeconds(duration);
        
        // Restaurar control
        player.enabled = wasEnabled;
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

    private void OnDrawGizmosSelected()
    {
        // Visualizar área de abrazo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, embraceDistance);
    }
}
