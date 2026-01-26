using System.Collections;
using UnityEngine;

/// <summary>
/// GhostLiberation - Maneja la mecánica de liberación (paz) o destrucción (dolor) de fantasmas.
/// </summary>
public class GhostLiberation : MonoBehaviour
{
    public enum LiberationMode { Peaceful, Intense }

    [Header("Configuración General")]
    [SerializeField] private float peacefulDuration = 2.5f;
    [SerializeField] private float intenseDuration = 1.2f; // Un poco más para ver el "sufrimiento"
    [SerializeField] private float ascendHeight = 4f;
    
    [Header("Colores")]
    [SerializeField] private Color peacefulColor = Color.cyan;
    [SerializeField] private Color intenseColor = Color.red; // Color de "daño/quemadura"
    
    private bool isBeingLiberated = false;
    public bool IsBeingLiberated => isBeingLiberated;
    private GhostAI ghostAI;
    private SpriteRenderer sprite;

    private void Awake()
    {
        ghostAI = GetComponent<GhostAI>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Liberate(LiberationMode mode)
    {
        if (isBeingLiberated) return;
        isBeingLiberated = true;
        
        if (ghostAI != null) ghostAI.enabled = false;

        StartCoroutine(LiberationEffect(mode));
    }

    private IEnumerator LiberationEffect(LiberationMode mode)
    {
        if (sprite == null) { Destroy(gameObject); yield break; }
        
        float elapsed = 0f;
        float duration = (mode == LiberationMode.Peaceful) ? peacefulDuration : intenseDuration;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Color originalColor = sprite.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (mode == LiberationMode.Peaceful)
            {
                // MODO FUENTE: Asciende y se desvanece suavemente (Paz)
                transform.position = startPos + Vector3.up * t * ascendHeight;
                sprite.color = Color.Lerp(originalColor, new Color(peacefulColor.r, peacefulColor.g, peacefulColor.b, 0), t);
                transform.Rotate(Vector3.forward * Time.deltaTime * 50f);
            }
            else
            {
                // MODO LUZ INTENSA: Destrucción por daño (Sufrimiento)
                
                // 1. Vibración Violenta (Shake)
                float shakeAmount = 0.2f * (t + 0.5f); // Aumenta con el tiempo
                transform.position = startPos + (Vector3)Random.insideUnitCircle * shakeAmount;

                // 2. Parpadeo de color "Quemadura" (Rojo/Blanco intensos)
                float flash = Mathf.Abs(Mathf.Sin(elapsed * 25f)); // Parpadeo muy rápido
                sprite.color = Color.Lerp(originalColor, Color.white, flash);
                if (flash > 0.5f) sprite.color = intenseColor;

                // 3. Deformación (Jitter de escala)
                float jitter = 1f + Mathf.Sin(elapsed * 50f) * 0.2f;
                transform.localScale = new Vector3(startScale.x * jitter, startScale.y * (2f - jitter), startScale.z);

                // 4. Encogimiento final agónico
                if (t > 0.8f)
                {
                    float finalT = (t - 0.8f) * 5f;
                    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, finalT);
                }
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
