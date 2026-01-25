using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Tipo de Transición")]
    [Tooltip("Si está marcado, ignorará 'Scene To Load' y cargará 'Cuadrante + Nivel Actual'.")]
    [SerializeField] private bool useLevelProgression = false;
    
    [Tooltip("Si quieres avanzar al siguiente nivel al cruzar esta puerta, marca esto.")]
    [SerializeField] private bool incrementLevelOnExit = false;

    [Header("Escena Fija (si no usas Progresión)")]
    [Tooltip("Nombre de la escena a la que quieres ir (ej: DentroDelHub2)")]
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

            // 1. Guardar el ID de aparición
            GameManager.Instance.LastExitUsed = targetSpawnID;

            if (saveOnExit) GameManager.Instance.SaveGame();

            // 2. Determinar qué escena cargar
            string finalScene = sceneToLoad;

            if (useLevelProgression)
            {
                // Si usamos progresión, la escena será "Cuadrante" + el nivel actual
                finalScene = "Cuadrante" + GameManager.Instance.CurrentLevel;
            }

            // 3. Cargar escena (CON o SIN transición)
            if (!string.IsNullOrEmpty(finalScene))
            {
                if (incrementLevelOnExit)
                {
                    // FINAL DE NIVEL → Con transición narrativa
                    int nivelCompletado = GameManager.Instance.CurrentLevel;
                    GameManager.Instance.NextLevel(); // Avanzar antes de la transición
                    GameManager.Instance.IrATransicion(nivelCompletado, finalScene);
                }
                else
                {
                    // PUERTA DEL HUB → Sin transición (carga directa)
                    Debug.Log($"[LevelExit] Transición directa a: {finalScene}");
                    SceneManager.LoadScene(finalScene);
                }
            }
        }
    }
}