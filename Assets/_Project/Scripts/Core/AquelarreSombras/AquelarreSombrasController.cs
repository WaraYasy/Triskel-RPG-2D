using UnityEngine;
using Triskel.API;

/// <summary>
/// AquelarreSombrasController - Controlador del nivel 3 (Aquelarre de Sombras).
/// Trackea la decisión moral: activar el altar de la verdad (revelar) vs no activarlo (ocultar).
/// </summary>
public class AquelarreSombrasController : MonoBehaviour
{
    /// <summary>
    /// Determina la decisión moral final basándose en si el altar fue activado.
    /// Llamar al completar el nivel para registrar la decisión en la API.
    /// </summary>
    /// <returns>La decisión moral: "revelar" si activó el altar, "ocultar" en caso contrario</returns>
    public string GetFinalMoralChoice()
    {
        // Usar el método estático de TruthAltar para verificar si fue activado
        bool altarActivated = TruthAltar.IsTruthRevealed;

        if (altarActivated)
        {
            Debug.Log("[AquelarreSombras] Decisión BUENA: Altar activado → REVELAR");
            return APIConstants.Choices.REVELAR;
        }
        else
        {
            Debug.Log("[AquelarreSombras] Decisión MALA: Altar NO activado → OCULTAR");
            return APIConstants.Choices.OCULTAR;
        }
    }

    [ContextMenu("Debug: Estado del Nivel")]
    public void DebugLevelState()
    {
        Debug.Log("===== ESTADO AQUELARRE DE SOMBRAS =====");
        Debug.Log($"Altar de la Verdad: {(TruthAltar.IsTruthRevealed ? "ACTIVADO" : "NO ACTIVADO")}");
        Debug.Log($"Decisión Final: {GetFinalMoralChoice()}");
        Debug.Log("=======================================");
    }
}

