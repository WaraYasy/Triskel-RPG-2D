using UnityEngine;
using Triskel.Core;

/// <summary>
/// LiberationFountain - Zona donde se liberan los fantasmas.
/// </summary>
public class LiberationFountain : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int ghostsRequiredForMoral = 4;
    [SerializeField] private int ghostsLiberated = 0;
    
    [Header("Efectos")]
    [SerializeField] private ParticleSystem liberationParticles;

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
        // Intentar obtener el script de liberación para el efecto visual si existe
        GhostLiberation liberation = ghostObj.GetComponent<GhostLiberation>();
        
        if (liberation != null)
        {
            // Llamamos a un método de liberación que no dé moral directamente
            liberation.LiberateAtFountain();
        }
        else
        {
            Destroy(ghostObj);
        }

        ghostsLiberated++;
        Debug.Log($"[Fountain] Fantasma liberado. Total: {ghostsLiberated}/{ghostsRequiredForMoral}");

        if (ghostsLiberated >= ghostsRequiredForMoral)
        {
            AwardMoral();
            ghostsLiberated = 0; // Reiniciar o lo que prefieras
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
            GameManager.Instance.ModifyMoral(1);
            Debug.Log("[Fountain] ¡4 fantasmas liberados! +1 Moral concedida.");
        }
    }
}
