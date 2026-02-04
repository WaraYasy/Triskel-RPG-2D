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
    
    // Trackea la contribución moral actual de este nivel (-1, 0, o +1)
    // Para evitar inflar la moral global infinita
    private int currentLevelMoralContribution = 0;

    /// <summary>
    /// Llamar cuando un fantasma es liberado en la fuente.
    /// </summary>
    public void OnGhostLiberated()
    {
        ghostsLiberated++;
        Debug.Log($"[SendaEbano] Fantasma liberado. Total: {ghostsLiberated} liberados, {ghostsKilled} matados");
        UpdateMoralState();
    }

    /// <summary>
    /// Llamar cuando un fantasma es matado con luz intensa.
    /// </summary>
    public void OnGhostKilled()
    {
        ghostsKilled++;
        Debug.Log($"[SendaEbano] Fantasma matado. Total: {ghostsLiberated} liberados, {ghostsKilled} matados");
        UpdateMoralState();
    }

    /// <summary>
    /// Actualiza la moral global basada en el estado actual del nivel.
    /// Regla: 
    /// - Si matas 2 o más fantasmas -> Moral Mala (-1).
    /// - Si matas < 2 y has liberado al menos 1 -> Moral Buena (+1).
    /// - Si no has hecho nada -> Moral Neutra (0).
    /// </summary>
    private void UpdateMoralState()
    {
        int desiredContribution = 0;

        if (ghostsKilled >= 2)
        {
            desiredContribution = -1;
        }
        else if (ghostsLiberated > 0)
        {
            desiredContribution = 1;
        }

        // Aplicar la diferencia si ha cambiado
        if (desiredContribution != currentLevelMoralContribution)
        {
            int diff = desiredContribution - currentLevelMoralContribution;
            currentLevelMoralContribution = desiredContribution;

            if (GameManager.Instance != null && diff != 0)
            {
                GameManager.Instance.ModifyMoral(diff);
                Debug.Log($"[SendaEbano] Moral actualizada. Contribución nivel: {currentLevelMoralContribution} (Diff: {diff})");
            }
        }
    }

    /// <summary>
    /// Determina la decisión moral final basándose en las acciones del jugador.
    /// Llamar al completar el nivel para registrar la decisión en la API.
    /// </summary>
    /// <returns>La decisión moral: "sanar" si liberó más fantasmas, "forzar" en caso contrario</returns>
    public string GetFinalMoralChoice()
    {
        // Regla solicitada: "solo es moral baja si matas a dos o mas fantasmas"
        // Si matas < 2 (0 o 1) se considera BUENO (SANAR), asumiendo que el resto son liberados o ignorados.
        if (ghostsKilled >= 2)
        {
            Debug.Log($"[SendaEbano] Decisión MALA: Matados ({ghostsKilled}) >= 2 → FORZAR");
            return APIConstants.Choices.FORZAR;
        }
        else
        {
            Debug.Log($"[SendaEbano] Decisión BUENA: Matados ({ghostsKilled}) < 2 → SANAR");
            return APIConstants.Choices.SANAR;
        }
    }

    [ContextMenu("Debug: Estado del Nivel")]
    public void DebugLevelState()
    {
        Debug.Log("===== ESTADO SENDA DEL ÉBANO =====");
        Debug.Log($"Fantasmas Liberados: {ghostsLiberated}");
        Debug.Log($"Fantasmas Matados: {ghostsKilled}");
        Debug.Log($"Contribución Moral: {currentLevelMoralContribution}");
        Debug.Log($"Decisión Final: {GetFinalMoralChoice()}");
        Debug.Log("==================================");
    }

    public int GetGhostsLiberated() => ghostsLiberated;
    public int GetGhostsKilled() => ghostsKilled;

    /// <summary>
    /// Finaliza el nivel y envía los datos a la API.
    /// Llamar al terminar el nivel (ej: al llegar a la salida).
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log("[SendaEbano] Completando nivel explícitamente...");
        if (GameManager.Instance != null && GameManager.Instance.GetAPITracker() != null)
        {
            GameManager.Instance.GetAPITracker().OnLevelComplete();
        }
        else
        {
            Debug.LogWarning("[SendaEbano] No se pudo completar nivel: GameManager o API Tracker null");
        }
    }
}
