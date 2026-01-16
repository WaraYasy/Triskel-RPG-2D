using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// LevelExit - Gestiona la transición del Hub a los niveles principales.
/// Se basa en el nivel actual guardado en el GameManager.
/// </summary>
public class LevelExit : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool saveOnExit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log para saber que el trigger funciona
        Debug.Log($"[LevelExit] Objeto detectado: {other.name}");

        if (other.CompareTag(playerTag))
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[LevelExit] ¡ERROR! No se encuentra el GameManager en la escena. Asegúrate de que exista un objeto con el script GameManager.");
                return;
            }

            int levelToLoad = GameManager.Instance.CurrentLevel;
            string sceneName = "Cuadrante" + levelToLoad;
            
            if (saveOnExit)
            {
                GameManager.Instance.SaveGame();
            }
            
            Debug.Log($"[LevelExit] ¡Cargando nivel {levelToLoad}! Escena: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
    }
}
