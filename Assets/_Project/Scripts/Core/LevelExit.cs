using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelExit - Gestiona el cambio a una escena específica.
/// </summary>
public class LevelExit : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la escena a la que quieres ir (ej: DentroDelHub2)")]
    [SerializeField] private string sceneToLoad = "DentroDelHub2";
    [Tooltip("ID de aparición en la siguiente escena (ej: Puerta_Casa)")]
    [SerializeField] private string targetSpawnID = "";
    
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool saveOnExit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Guardar el ID de la puerta en el GameManager para que la siguiente escena sepa dónde ponernos
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LastExitUsed = targetSpawnID;
                
                if (saveOnExit)
                {
                    GameManager.Instance.SaveGame();
                }
            }
            
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.Log($"[LevelExit] Guardando salida: {targetSpawnID}. Cargando escena: {sceneToLoad}");
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("[LevelExit] ¡No has puesto nombre de escena en Scene To Load!");
            }
        }
    }
}
