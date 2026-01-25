using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tipos de salida/transición entre escenas
/// </summary>
public enum ExitType
{
    HubDoor,        // Puerta del hub (carga directa, sin transición)
    HubToLevel,     // Hub → Nivel (con transición "nivel0")
    LevelComplete   // Nivel completado → Siguiente nivel (con transición según moral)
}

public class LevelExit : MonoBehaviour
{
    [Header("Tipo de Salida")]
    [Tooltip("HubDoor: Entre habitaciones del hub (sin transición)\nHubToLevel: Del hub al nivel (transición 'nivel0')\nLevelComplete: Completar nivel (transición según moral + avanza nivel)")]
    [SerializeField] private ExitType exitType = ExitType.HubDoor;

    [Header("Configuración")]
    [Tooltip("Usa progresión automática (Cuadrante + número). Si está desmarcado, usa 'Scene To Load'")]
    [SerializeField] private bool useLevelProgression = true;

    [Tooltip("Nombre de escena fija (solo si useLevelProgression = false)")]
    [SerializeField] private string sceneToLoad = "";

    [Header("Punto de Aparición")]
    [Tooltip("ID de aparición en la siguiente escena (ej: Puerta_Casa)")]
    [SerializeField] private string targetSpawnID = "";

    [Header("Ajustes")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool saveOnExit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (GameManager.Instance == null) return;

            // Guardar punto de aparición
            GameManager.Instance.LastExitUsed = targetSpawnID;

            if (saveOnExit)
                GameManager.Instance.SaveGame();

            // Procesar según tipo de salida
            switch (exitType)
            {
                case ExitType.HubDoor:
                    HandleHubDoor();
                    break;

                case ExitType.HubToLevel:
                    HandleHubToLevel();
                    break;

                case ExitType.LevelComplete:
                    HandleLevelComplete();
                    break;
            }
        }
    }

    /// <summary>
    /// Puerta entre habitaciones del hub (sin transición)
    /// </summary>
    private void HandleHubDoor()
    {
        string targetScene = useLevelProgression ?
            "Cuadrante" + GameManager.Instance.CurrentLevel :
            sceneToLoad;

        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"[LevelExit] Hub Door → {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogError("[LevelExit] No se especificó escena destino");
        }
    }

    /// <summary>
    /// Salida del hub hacia un nivel (con transición nivel0)
    /// </summary>
    private void HandleHubToLevel()
    {
        string targetScene = useLevelProgression ?
            "Cuadrante" + GameManager.Instance.CurrentLevel :
            sceneToLoad;

        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"[LevelExit] Hub → Level: {targetScene} (mostrando transición nivel0)");
            GameManager.Instance.IrATransicion(0, targetScene);
        }
        else
        {
            Debug.LogError("[LevelExit] No se especificó escena destino");
        }
    }

    /// <summary>
    /// Nivel completado (con transición según moral + avanza nivel)
    /// </summary>
    private void HandleLevelComplete()
    {
        int nivelCompletado = GameManager.Instance.CurrentLevel;
        GameManager.Instance.NextLevel(); // Avanzar al siguiente nivel

        string targetScene = useLevelProgression ?
            "Cuadrante" + GameManager.Instance.CurrentLevel :
            sceneToLoad;

        if (!string.IsNullOrEmpty(targetScene))
        {
            Debug.Log($"[LevelExit] Level Complete: Nivel {nivelCompletado} → {targetScene}");
            GameManager.Instance.IrATransicion(nivelCompletado, targetScene);
        }
        else
        {
            Debug.LogError("[LevelExit] No se especificó escena destino");
        }
    }
}