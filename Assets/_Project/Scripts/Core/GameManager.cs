using UnityEngine;
using UnityEngine.SceneManagement;
using Triskel.Core;

/// <summary>
/// GameManager - Singleton que persiste entre escenas.
/// Orquesta los sistemas de Inventario y Diario.
/// Versión 2.0 - Orquestador Simple
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ===== REFERENCIAS A SISTEMAS =====
    private DiaryManager diaryManager;
    private InventoryData inventoryData;
    private InventoryPersistence inventoryPersistence;
    private DiaryPersistence diaryPersistence;

    // ===== ESTADO DEL JUEGO =====
    [Header("Estado del Juego")]
    [SerializeField] private int moralScore = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private string hubSceneName = "Hub"; // Nombre de la escena del Hub
    [SerializeField] private string lastExitUsed = ""; // Rastrae qué puerta usamos

    // ===== PROPIEDADES PÚBLICAS =====
    public int MoralScore => moralScore;
    public int CurrentLevel => currentLevel;
    public string LastExitUsed { get => lastExitUsed; set => lastExitUsed = value; }

    #region Unity Lifecycle

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

    private void Start()
    {
        // Obtener referencias DESPUÉS de que todos los Awake() terminen
        InitializeManagers();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Obtiene referencias a todos los managers del juego.
    /// </summary>
    private void InitializeManagers()
    {
        diaryManager = FindFirstObjectByType<DiaryManager>();
        inventoryData = FindFirstObjectByType<InventoryData>();
        inventoryPersistence = FindFirstObjectByType<InventoryPersistence>();
        diaryPersistence = FindFirstObjectByType<DiaryPersistence>();

        // Validación
        if (diaryManager == null)
            Debug.LogWarning("[GameManager] DiaryManager no encontrado.");
        if (inventoryData == null)
            Debug.LogWarning("[GameManager] InventoryData no encontrado.");
        if (inventoryPersistence == null)
            Debug.LogWarning("[GameManager] InventoryPersistence no encontrado.");
        if (diaryPersistence == null)
            Debug.LogWarning("[GameManager] DiaryPersistence no encontrado.");

        Debug.Log($"[GameManager] Inicializado. Diary={diaryManager != null}, Inventory={inventoryData != null}");
    }

    #endregion

    #region Save/Load System

    /// <summary>
    /// Guarda el estado completo del juego (inventario + diario + estado global).
    /// </summary>
    public void SaveGame()
    {
        Debug.Log("[GameManager] Guardando partida...");

        // 1. Guardar inventario
        if (inventoryPersistence != null)
        {
            inventoryPersistence.SaveInventory();
        }

        // 2. Guardar diario
        if (diaryManager != null && diaryPersistence != null)
        {
            var unlockedEntries = diaryManager.GetUnlockedEntries();
            if (unlockedEntries != null && unlockedEntries.Count > 0)
            {
                var unlockedIDs = new string[unlockedEntries.Count];
                for (int i = 0; i < unlockedEntries.Count; i++)
                {
                    unlockedIDs[i] = unlockedEntries[i].id;
                }
                diaryPersistence.SaveUnlockedEntries(unlockedIDs);
            }
            else
            {
                // Si no hay entradas, guardar array vacío
                diaryPersistence.SaveUnlockedEntries(new string[0]);
            }
        }

        // 3. Guardar estado global
        SaveToLocal();

        Debug.Log($"[GameManager] ✓ Partida guardada (Moral={moralScore}, Nivel={currentLevel})");
    }

    /// <summary>
    /// Carga el estado completo del juego.
    /// </summary>
    public void LoadGame()
    {
        Debug.Log("[GameManager] Cargando partida...");

        // 1. Cargar estado global
        LoadFromLocal();

        // 2. Cargar inventario
        if (inventoryPersistence != null)
        {
            inventoryPersistence.LoadInventory();
        }

        // 3. Cargar diario
        if (diaryManager != null && diaryPersistence != null)
        {
            string[] unlockedIDs = diaryPersistence.LoadUnlockedEntries();
            foreach (string id in unlockedIDs)
            {
                diaryManager.RestoreEntry(id);
            }
        }

        int itemCount = inventoryData?.GetAllItems().Count ?? 0;
        int diaryCount = diaryManager?.GetUnlockedEntries().Count ?? 0;

        Debug.Log($"[GameManager] ✓ Partida cargada: Moral={moralScore}, Nivel={currentLevel}, Items={itemCount}, Entradas={diaryCount}");

        // Tras cargar los datos, enviamos al jugador al HUB
        if (!string.IsNullOrEmpty(hubSceneName))
        {
            Debug.Log($"[GameManager] Volviendo al Hub: {hubSceneName}");
            SceneManager.LoadScene(hubSceneName);
        }
    }

    /// <summary>
    /// Inicia una nueva partida, limpiando todo el estado previo.
    /// </summary>
    public void NewGame()
    {
        Debug.Log("[GameManager] Iniciando nueva partida...");

        // 1. Limpiar estado global
        moralScore = 0;
        currentLevel = 1;

        // 2. Limpiar inventario
        if (inventoryData != null)
        {
            inventoryData.ClearInventory();
        }
        if (inventoryPersistence != null)
        {
            inventoryPersistence.ClearSavedData();
        }

        // 3. Limpiar diario
        if (diaryManager != null)
        {
            diaryManager.ClearAll();
        }
        if (diaryPersistence != null)
        {
            diaryPersistence.ClearSavedData();
        }

        // 4. Limpiar PlayerPrefs
        PlayerPrefs.DeleteKey("game_moral");
        PlayerPrefs.DeleteKey("game_level");
        PlayerPrefs.Save();

        Debug.Log("[GameManager] ✓ Nueva partida iniciada");
    }

    #endregion

    #region Game State

    /// <summary>
    /// Modifica la moral del jugador.
    /// </summary>
    /// <param name="delta">Cambio en moral (+1, -1, etc.)</param>
    public void ModifyMoral(int delta)
    {
        int oldMoral = moralScore;
        moralScore = Mathf.Clamp(moralScore + delta, -10, 10);

        if (moralScore != oldMoral)
        {
            Debug.Log($"[GameManager] Moral: {oldMoral} → {moralScore} ({(delta > 0 ? "+" : "")}{delta})");
        }
    }

    /// <summary>
    /// Avanza al siguiente nivel.
    /// </summary>
    public void NextLevel()
    {
        currentLevel++;
        Debug.Log($"[GameManager] Nivel actual: {currentLevel}");
    }

    #endregion

    #region Local Persistence (PlayerPrefs)

    private void SaveToLocal()
    {
        PlayerPrefs.SetInt("game_moral", moralScore);
        PlayerPrefs.SetInt("game_level", currentLevel);
        PlayerPrefs.Save();
    }

    
    private void LoadFromLocal()
    {
        moralScore = PlayerPrefs.GetInt("game_moral", 0);
        currentLevel = PlayerPrefs.GetInt("game_level", 1);
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Imprime el estado completo del juego en la consola.
    /// </summary>
    [ContextMenu("Debug: Estado Completo")]
    public void DebugPrintState()
    {
        Debug.Log("===== ESTADO DEL JUEGO =====");
        Debug.Log($"Moral: {moralScore}");
        Debug.Log($"Nivel actual: {currentLevel}");
        Debug.Log($"Items inventario: {inventoryData?.GetAllItems().Count ?? 0}");
        Debug.Log($"Entradas diario: {diaryManager?.GetUnlockedEntries().Count ?? 0}");
        Debug.Log("============================");
    }

    /// <summary>
    /// Test de guardar/cargar.
    /// </summary>
    [ContextMenu("Debug: Test Save/Load")]
    public void DebugTestSaveLoad()
    {
        Debug.Log("[TEST] Guardando estado actual...");
        SaveGame();

        Debug.Log("[TEST] Creando nueva partida (limpia todo)...");
        NewGame();

        Debug.Log("[TEST] Cargando estado guardado...");
        LoadGame();

        Debug.Log("[TEST] Verificando estado restaurado:");
        DebugPrintState();
    }

    #endregion
}
