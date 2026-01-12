using UnityEngine;

/// <summary>
/// GameManager - Singleton que persiste entre escenas
/// Versión 1.0 - Solo el patrón Singleton básico
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // TODO: Aquí irán las variables del juego (moral, reliquias, etc.)
}
