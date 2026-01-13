using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Triskel.Core;
using Triskel.API;

/// <summary>
/// GameManager - Orquestador central del juego (Singleton).
/// Coordina todos los sistemas (Inventario, Diario, API) y gestiona el estado global.
/// Versión 2.0 - GameManager Orquestador
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ===== REFERENCIAS A SISTEMAS =====
    private DiaryManager diaryManager;
    private InventoryData inventoryData;
    private InventoryPersistence inventoryPersistence;
    private DiaryPersistence diaryPersistence;
    private TriskelAPIClient apiClient;

    // ===== ESTADO NARRATIVO =====
    [Header("Estado Narrativo")]
    [SerializeField] private int moralScore = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private string[] levelsCompleted = new string[0];

    // ===== PROGRESO TEMPORAL =====
    [Header("Progreso Temporal")]
    [SerializeField] private float totalPlayTime = 0f;
    private float sessionStartTime;

    // ===== ESTADO DE LA PARTIDA =====
    [Header("Estado de la Partida")]
    [SerializeField] private bool bossDefeated = false;
    [SerializeField] private string gameStatus = "in_progress"; // "in_progress", "completed", "abandoned"

    // ===== SINCRONIZACIÓN =====
    [Header("Sincronización")]
    [SerializeField] private bool autoSyncWithAPI = true;
    [SerializeField] private float autoSaveInterval = 60f; // Auto-guardar cada 60 segundos

    // ===== EVENTOS PÚBLICOS =====
    public event Action OnGameSaved;
    public event Action OnGameLoaded;
    public event Action<int> OnMoralChanged;
    public event Action<int> OnLevelCompleted;

    // ===== PROPIEDADES PÚBLICAS (READ-ONLY) =====
    public int MoralScore => moralScore;
    public int CurrentLevel => currentLevel;
    public float TotalPlayTime => totalPlayTime;
    public bool BossDefeated => bossDefeated;
    public string GameStatus => gameStatus;

    // ===== VARIABLES DE AUTO-GUARDADO =====
    private float lastAutoSaveTime = 0f;

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        sessionStartTime = Time.time;
        Debug.Log("[GameManager] Inicializado. Usa LoadGame() para cargar partida guardada.");
    }

    private void Update()
    {
        // Auto-guardar periódicamente
        if (gameStatus == "in_progress" && Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            SaveGame(syncWithAPI: false); // Solo local, no saturar API
            lastAutoSaveTime = Time.time;
        }
    }

    private void OnApplicationQuit()
    {
        // Guardar al cerrar el juego
        if (gameStatus == "in_progress")
        {
            SaveGame(syncWithAPI: autoSyncWithAPI);
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Obtiene referencias automáticas a todos los managers del juego.
    /// </summary>
    private void InitializeManagers()
    {
        diaryManager = FindFirstObjectByType<DiaryManager>();
        inventoryData = FindFirstObjectByType<InventoryData>();
        inventoryPersistence = FindFirstObjectByType<InventoryPersistence>();
        diaryPersistence = FindFirstObjectByType<DiaryPersistence>();
        apiClient = FindFirstObjectByType<TriskelAPIClient>();

        // Validación con logs
        if (diaryManager == null)
            Debug.LogWarning("[GameManager] DiaryManager no encontrado. El sistema de diario no funcionará.");
        if (inventoryData == null)
            Debug.LogWarning("[GameManager] InventoryData no encontrado. El sistema de inventario no funcionará.");
        if (inventoryPersistence == null)
            Debug.LogWarning("[GameManager] InventoryPersistence no encontrado.");
        if (diaryPersistence == null)
            Debug.LogWarning("[GameManager] DiaryPersistence no encontrado.");
        if (apiClient == null)
            Debug.LogWarning("[GameManager] TriskelAPIClient no encontrado. La sincronización remota no funcionará.");

        Debug.Log($"[GameManager] Managers inicializados: " +
                  $"Diary={diaryManager != null}, " +
                  $"Inventory={inventoryData != null}, " +
                  $"Persistence={inventoryPersistence != null}, " +
                  $"API={apiClient != null}");
    }

    #endregion

    #region Save/Load System

    /// <summary>
    /// Guarda el estado completo del juego (inventario, diario, estado global).
    /// </summary>
    /// <param name="syncWithAPI">Si debe sincronizar con la API remota</param>
    public void SaveGame(bool syncWithAPI = true)
    {
        Debug.Log("[GameManager] Guardando partida...");

        // 1. Actualizar tiempo jugado
        totalPlayTime += Time.time - sessionStartTime;
        sessionStartTime = Time.time;

        // 2. Guardar inventario (via InventoryPersistence)
        if (inventoryPersistence != null)
        {
            inventoryPersistence.SaveInventory();
        }

        // 3. Guardar diario (via DiaryPersistence)
        if (diaryManager != null && diaryPersistence != null)
        {
            string[] unlockedIDs = diaryManager.GetUnlockedEntries()
                .Select(e => e.id)
                .ToArray();
            diaryPersistence.SaveUnlockedEntries(unlockedIDs);
        }

        // 4. Guardar estado global (PlayerPrefs)
        SaveToLocal();

        // 5. Sincronizar con API (opcional)
        if (syncWithAPI && autoSyncWithAPI && apiClient != null)
        {
            SyncToRemote();
        }

        OnGameSaved?.Invoke();
        Debug.Log($"[GameManager] ✓ Partida guardada (Moral={moralScore}, Nivel={currentLevel}, Tiempo={totalPlayTime:F1}s)");
    }

    /// <summary>
    /// Carga el estado completo del juego.
    /// Intenta cargar de API primero (si está online), sino de PlayerPrefs.
    /// </summary>
    public void LoadGame()
    {
        Debug.Log("[GameManager] Cargando partida...");

        // 1. Cargar estado global (PlayerPrefs)
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

        sessionStartTime = Time.time;
        OnGameLoaded?.Invoke();

        int itemCount = inventoryData?.GetAllItems().Count ?? 0;
        int diaryCount = diaryManager?.GetUnlockedEntries().Count ?? 0;

        Debug.Log($"[GameManager] ✓ Partida cargada: Moral={moralScore}, " +
                  $"Nivel={currentLevel}, " +
                  $"Items={itemCount}, " +
                  $"Entradas diario={diaryCount}, " +
                  $"Tiempo={totalPlayTime:F1}s");
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
        levelsCompleted = new string[0];
        totalPlayTime = 0f;
        bossDefeated = false;
        gameStatus = "in_progress";
        sessionStartTime = Time.time;

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

        // 4. Limpiar PlayerPrefs del GameManager
        PlayerPrefs.DeleteKey("game_moral");
        PlayerPrefs.DeleteKey("game_level");
        PlayerPrefs.DeleteKey("game_playtime");
        PlayerPrefs.DeleteKey("game_status");
        PlayerPrefs.DeleteKey("game_levels_completed");
        PlayerPrefs.DeleteKey("game_boss_defeated");
        PlayerPrefs.Save();

        Debug.Log("[GameManager] ✓ Nueva partida iniciada");
    }

    #endregion

    #region State Management

    /// <summary>
    /// Modifica la moral del jugador (impacta dificultad del boss).
    /// </summary>
    /// <param name="delta">Cambio en moral (+1, -1, etc.)</param>
    public void ModifyMoral(int delta)
    {
        int oldMoral = moralScore;
        moralScore = Mathf.Clamp(moralScore + delta, -10, 10); // Límites: -10 a +10

        if (moralScore != oldMoral)
        {
            Debug.Log($"[GameManager] Moral: {oldMoral} → {moralScore} ({(delta > 0 ? "+" : "")}{delta})");
            OnMoralChanged?.Invoke(moralScore);

            // Auto-guardar después de cambio importante
            SaveGame();
        }
    }

    /// <summary>
    /// Marca un nivel como completado.
    /// </summary>
    public void CompleteLevel(int levelNumber)
    {
        string levelID = $"level{levelNumber}";

        if (!System.Array.Exists(levelsCompleted, l => l == levelID))
        {
            var tempList = new List<string>(levelsCompleted);
            tempList.Add(levelID);
            levelsCompleted = tempList.ToArray();

            currentLevel = levelNumber + 1; // Avanzar al siguiente

            Debug.Log($"[GameManager] Nivel {levelNumber} completado. Siguiente nivel: {currentLevel}");
            OnLevelCompleted?.Invoke(levelNumber);

            SaveGame(); // Auto-guardar al completar nivel
        }
    }

    /// <summary>
    /// Marca al jefe final como derrotado y completa la partida.
    /// </summary>
    public void SetBossDefeated()
    {
        bossDefeated = true;
        gameStatus = "completed";

        Debug.Log("[GameManager] ¡JEFE DERROTADO! Partida completada");
        SaveGame(); // Guardar victoria
    }

    #endregion

    #region Local Persistence (PlayerPrefs)

    private void SaveToLocal()
    {
        PlayerPrefs.SetInt("game_moral", moralScore);
        PlayerPrefs.SetInt("game_level", currentLevel);
        PlayerPrefs.SetFloat("game_playtime", totalPlayTime);
        PlayerPrefs.SetString("game_status", gameStatus);
        PlayerPrefs.SetString("game_levels_completed", string.Join(",", levelsCompleted));
        PlayerPrefs.SetInt("game_boss_defeated", bossDefeated ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadFromLocal()
    {
        moralScore = PlayerPrefs.GetInt("game_moral", 0);
        currentLevel = PlayerPrefs.GetInt("game_level", 1);
        totalPlayTime = PlayerPrefs.GetFloat("game_playtime", 0f);
        gameStatus = PlayerPrefs.GetString("game_status", "in_progress");

        string levelsCSV = PlayerPrefs.GetString("game_levels_completed", "");
        levelsCompleted = string.IsNullOrEmpty(levelsCSV) ? new string[0] : levelsCSV.Split(',');

        bossDefeated = PlayerPrefs.GetInt("game_boss_defeated", 0) == 1;
    }

    #endregion

    #region Remote Sync (API)

    private void SyncToRemote()
    {
        if (apiClient == null || string.IsNullOrEmpty(apiClient.currentGameID))
        {
            Debug.LogWarning("[GameManager] No se puede sincronizar con API: gameID no configurado");
            return;
        }

        // Obtener IDs de reliquias del inventario
        string[] relics = inventoryData != null ? inventoryData.GetItemIDs() : new string[0];

        // Actualizar reliquias en API
        apiClient.UpdateGameRelics(relics,
            onSuccess: () => Debug.Log("[GameManager] Sincronizado con API"),
            onError: (err) => Debug.LogWarning($"[GameManager] Error al sincronizar con API: {err}")
        );

        // TODO: Cuando la API soporte más campos, sincronizar también:
        // - moralScore
        // - levelsCompleted
        // - bossDefeated
        // - gameStatus
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Imprime el estado completo del juego en la consola (para debugging).
    /// </summary>
    [ContextMenu("Debug: Estado Completo")]
    public void DebugPrintState()
    {
        Debug.Log("===== ESTADO DEL JUEGO =====");
        Debug.Log($"Moral: {moralScore}");
        Debug.Log($"Nivel actual: {currentLevel}");
        Debug.Log($"Niveles completados: {string.Join(", ", levelsCompleted)}");
        Debug.Log($"Tiempo jugado: {totalPlayTime:F1}s");
        Debug.Log($"Boss derrotado: {bossDefeated}");
        Debug.Log($"Estado: {gameStatus}");
        Debug.Log($"Items inventario: {inventoryData?.GetAllItems().Count ?? 0}");
        Debug.Log($"Entradas diario: {diaryManager?.GetUnlockedEntries().Count ?? 0}");
        Debug.Log("============================");
    }

    /// <summary>
    /// Test completo de guardar/cargar (para debugging).
    /// </summary>
    [ContextMenu("Debug: Test Save/Load")]
    public void DebugTestSaveLoad()
    {
        Debug.Log("[GameManager] TEST: Guardando estado actual...");
        SaveGame();

        Debug.Log("[GameManager] TEST: Creando nueva partida (limpia todo)...");
        NewGame();

        Debug.Log("[GameManager] TEST: Cargando estado guardado...");
        LoadGame();

        Debug.Log("[GameManager] TEST: Verificando estado restaurado:");
        DebugPrintState();
    }

    #endregion
}
