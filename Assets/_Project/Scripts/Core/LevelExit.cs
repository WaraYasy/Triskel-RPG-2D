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

            // 1. ¿Debemos avanzar de nivel? (Ej: al terminar el nivel 1)
            if (incrementLevelOnExit)
            {
                GameManager.Instance.NextLevel();
            }

            // 2. Guardar el ID de aparición
            GameManager.Instance.LastExitUsed = targetSpawnID;
            
            if (saveOnExit) GameManager.Instance.SaveGame();

            // 3. Determinar qué escena cargar
            string finalScene = sceneToLoad;

            if (useLevelProgression)
            {
                // Si usamos progresión, la escena será "Cuadrante" + el nivel actual
                finalScene = "Cuadrante" + GameManager.Instance.CurrentLevel;
            }

            // 4. Cargar la escena
            if (!string.IsNullOrEmpty(finalScene))
            {
                Debug.Log($"[LevelExit] Transición a: {finalScene}");
                SceneManager.LoadScene(finalScene);
            }
        }
    }
}