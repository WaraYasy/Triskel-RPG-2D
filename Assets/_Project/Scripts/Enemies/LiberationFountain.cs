using UnityEngine;
using Triskel.Core;
using Triskel.API;

/// <summary>
/// LiberationFountain - Zona donde se liberan los fantasmas.
/// </summary>
public class LiberationFountain : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int ghostsRequiredForMoral = 4;
    [SerializeField] private int ghostsLiberated = 0;
    
    [Header("Efectos")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem liberationParticles;

    private readonly int liberateParam = Animator.StringToHash("Liberate");
    private readonly int activatedParam = Animator.StringToHash("Activated");

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si es un fantasma
        if (other.CompareTag("Ghost"))
        {
            LiberateGhost(other.gameObject);
        }
    }

    private void LiberateGhost(GameObject ghostObj)
    {
        // Intentar obtener el script de liberación
        GhostLiberation liberation = ghostObj.GetComponent<GhostLiberation>();

        // Evitar procesar el mismo fantasma si ya está en proceso de liberación
        if (liberation != null && liberation.IsBeingLiberated) return;

        if (liberation != null)
        {
            liberation.Liberate(GhostLiberation.LiberationMode.Peaceful);
        }
        else
        {
            Destroy(ghostObj);
        }

        ghostsLiberated++;
        Debug.Log($"[Fountain] Fantasma liberado. Total: {ghostsLiberated}/{ghostsRequiredForMoral}");

        // Notificar al controlador del nivel que se liberó un fantasma
        var sendaController = FindFirstObjectByType<SendaEbanoController>();
        if (sendaController != null)
        {
            sendaController.OnGhostLiberated();
        }

        if (ghostsLiberated >= ghostsRequiredForMoral)
        {
            AwardMoral();
            ghostsLiberated = 0;
        }
        else
        {
            // Solo disparamos la animación normal si NO hemos llegado al tope
            if (animator != null)
            {
                animator.SetTrigger(liberateParam);
            }
        }

        if (liberationParticles != null)
        {
            liberationParticles.Play();
        }
    }

    private void AwardMoral()
    {
        if (GameManager.Instance != null)
        {
            // Solo modificar moral local, la decisión se registrará al completar el nivel
            GameManager.Instance.ModifyMoral(1);
            Debug.Log("[Fountain] ¡4 fantasmas liberados! +1 Moral concedida.");
        }

        if (animator != null)
        {
            animator.SetTrigger(activatedParam);
        }
    }
}
