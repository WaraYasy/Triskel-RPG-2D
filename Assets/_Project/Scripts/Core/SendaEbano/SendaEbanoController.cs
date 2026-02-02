using UnityEngine;
using Triskel.API;

/// <summary>
/// SendaEbanoController - Controlador del nivel 1 (Senda del Ébano).
/// Trackea las decisiones morales: liberar fantasmas (sanar) vs matarlos (forzar).
/// </summary>
public class SendaEbanoController : MonoBehaviour
{
    private int ghostsLiberated = 0;
    private int ghostsKilled = 0;

    /// <summary>
    /// Llamar cuando un fantasma es liberado en la fuente.
    /// </summary>
    public void OnGhostLiberated()
    {
        ghostsLiberated++;
        Debug.Log($"[SendaEbano] Fantasma liberado. Total: {ghostsLiberated} liberados, {ghostsKilled} matados");
    }

    /// <summary>
    /// Llamar cuando un fantasma es matado con luz intensa.
    /// </summary>
    public void OnGhostKilled()
    {
        ghostsKilled++;
        Debug.Log($"[SendaEbano] Fantasma matado. Total: {ghostsLiberated} liberados, {ghostsKilled} matados");
    }

    /// <summary>
    /// Determina la decisión moral final basándose en las acciones del jugador.
    /// Llamar al completar el nivel para registrar la decisión en la API.
    /// </summary>
    /// <returns>La decisión moral: "sanar" si liberó más fantasmas, "forzar" en caso contrario</returns>
    public string GetFinalMoralChoice()
    {
        // Si liberó más fantasmas de los que mató → decisión buena (sanar)
        // En caso de empate, se considera mala decisión (forzar)
        if (ghostsLiberated > ghostsKilled)
        {
            Debug.Log($"[SendaEbano] Decisión BUENA: Liberados ({ghostsLiberated}) > Matados ({ghostsKilled}) → SANAR");
            return APIConstants.Choices.SANAR;
        }
        else
        {
            Debug.Log($"[SendaEbano] Decisión MALA: Matados ({ghostsKilled}) >= Liberados ({ghostsLiberated}) → FORZAR");
            return APIConstants.Choices.FORZAR;
        }
    }

    [ContextMenu("Debug: Estado del Nivel")]
    public void DebugLevelState()
    {
        Debug.Log("===== ESTADO SENDA DEL ÉBANO =====");
        Debug.Log($"Fantasmas Liberados: {ghostsLiberated}");
        Debug.Log($"Fantasmas Matados: {ghostsKilled}");
        Debug.Log($"Decisión Final: {GetFinalMoralChoice()}");
        Debug.Log("==================================");
    }

    public int GetGhostsLiberated() => ghostsLiberated;
    public int GetGhostsKilled() => ghostsKilled;
}
