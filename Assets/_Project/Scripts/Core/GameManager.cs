using _Project.Scripts.Core.Transition;
using UnityEngine;
using UnityEngine.SceneManagement;
using Triskel.Core;
using Triskel.API;
using Triskel.API.Models;

/// <summary>
/// GameManager - Singleton que persiste entre escenas.
/// Orquesta los sistemas de Inventario y Diario.
/// Versión 2.0 - Orquestador Simple
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ===== REFERENCIAS A SISTEMAS =====
    private GameplayAPITracker apiTracker;

    // ===== ESTADO DEL JUEGO =====
    [Header("Estado del Juego")]
    [SerializeField] private int moralScore = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private string hubSceneName = "Hub"; // Nombre de la escena del Hub
    [SerializeField] private string lastExitUsed = ""; // Rastrea qué puerta usamos

    // ===== PROPIEDADES PÚBLICAS =====
    public int MoralScore => moralScore;
    public int CurrentLevel => currentLevel;
    public string LastExitUsed { get => lastExitUsed; set => lastExitUsed = value; }

    /// <summary>
    /// Obtiene el tracker de API para registrar eventos de gameplay.
    /// </summary>
    public GameplayAPITracker GetAPITracker() => apiTracker;

    /// <summary>
    /// Elimina la reliquia correspondiente al nivel actual del inventario.
    /// </summary>
    /// <remarks>
    /// Se llama al morir o reiniciar un nivel para que el jugador pierda la reliquia
    /// recogida en ese nivel y tenga que recogerla de nuevo.
    ///
    /// Mapeo:
    /// - Nivel 1 (Cuadrante1/senda_ebano) → Lirio
    /// - Nivel 2 (Cuadrante2/fortaleza_gigantes) → Hacha
    /// - Nivel 3 (Cuadrante3/aquelarre_sombras) → Manto
    /// - Nivel 4 (Cuadrante4/claro_almas) → Sin reliquia
    /// </remarks>
    public void RemoveCurrentLevelRelic()
    {
        // Acceder directamente a Instance en lugar de usar la variable de campo
        // (más robusto cuando GameManager está en múltiples escenas)
        if (InventoryData.Instance == null)
        {
            Debug.LogWarning("[GameManager] InventoryData.Instance es null, no se puede eliminar reliquia");
            return;
        }

        string relicToRemove = null;

        Debug.Log($"[GameManager] Intentando eliminar reliquia del nivel {currentLevel}");

        // Determinar qué reliquia corresponde al nivel actual
        switch (currentLevel)
        {
            case 1:
                relicToRemove = "lirio";
                break;
            case 2:
                relicToRemove = "hacha";
                break;
            case 3:
                relicToRemove = "manto";
                break;
            case 4:
                // Nivel 4 no tiene reliquia específica
                Debug.Log("[GameManager] Nivel 4 no tiene reliquia específica");
                return;
            default:
                Debug.LogWarning($"[GameManager] Nivel {currentLevel} no reconocido");
                return;
        }

        // Mostrar items actuales en inventario antes de eliminar
        var items = InventoryData.Instance.GetAllItems();
        Debug.Log($"[GameManager] Items en inventario antes de eliminar: {items.Count}");
        foreach (var item in items)
        {
            Debug.Log($"  - {item.displayName} (ID: '{item.itemID}')");
        }

        // Intentar eliminar la reliquia del inventario
        if (!string.IsNullOrEmpty(relicToRemove))
        {
            Debug.Log($"[GameManager] Intentando eliminar reliquia con ID: '{relicToRemove}'");
            bool removed = InventoryData.Instance.RemoveItem(relicToRemove);
            if (removed)
            {
                Debug.Log($"[GameManager] ✓ Reliquia '{relicToRemove}' eliminada del inventario (reinicio de nivel {currentLevel})");
            }
            else
            {
                Debug.LogWarning($"[GameManager] ✗ No se pudo eliminar reliquia '{relicToRemove}' (no encontrada en inventario)");
            }
        }

        // Mostrar items después de eliminar
        items = InventoryData.Instance.GetAllItems();
        Debug.Log($"[GameManager] Items en inventario después de eliminar: {items.Count}");
    }

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
        InitializeAPITracker();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Inicializa el GameplayAPITracker para tracking de métricas.
    /// </summary>
    private void InitializeAPITracker()
    {
        // Buscar o crear GameplayAPITracker
        apiTracker = FindFirstObjectByType<GameplayAPITracker>();
        if (apiTracker == null)
        {
            apiTracker = gameObject.AddComponent<GameplayAPITracker>();
        }

        // Inicializar con dependencias (acceso directo a singletons)
        apiTracker.Initialize(TriskelAPIClient.Instance, InventoryData.Instance);
        Debug.Log("[GameManager] API Tracker inicializado.");
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
        if (InventoryPersistence.Instance != null)
        {
            InventoryPersistence.Instance.SaveInventory();
        }

        // 2. Guardar diario
        if (DiaryManager.Instance != null && DiaryPersistence.Instance != null)
        {
            var unlockedEntries = DiaryManager.Instance.GetUnlockedEntries();
            if (unlockedEntries != null && unlockedEntries.Count > 0)
            {
                var unlockedIDs = new string[unlockedEntries.Count];
                for (int i = 0; i < unlockedEntries.Count; i++)
                {
                    unlockedIDs[i] = unlockedEntries[i].id;
                }
                DiaryPersistence.Instance.SaveUnlockedEntries(unlockedIDs);
            }
            else
            {
                // Si no hay entradas, guardar array vacío
                DiaryPersistence.Instance.SaveUnlockedEntries(new string[0]);
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
        if (InventoryPersistence.Instance != null)
        {
            InventoryPersistence.Instance.LoadInventory();
        }

        // 3. Cargar diario
        if (DiaryManager.Instance != null && DiaryPersistence.Instance != null)
        {
            string[] unlockedIDs = DiaryPersistence.Instance.LoadUnlockedEntries();
            foreach (string id in unlockedIDs)
            {
                DiaryManager.Instance.RestoreEntry(id);
            }
        }

        int itemCount = InventoryData.Instance?.GetAllItems().Count ?? 0;
        int diaryCount = DiaryManager.Instance?.GetUnlockedEntries().Count ?? 0;

        Debug.Log($"[GameManager] ✓ Partida cargada: Moral={moralScore}, Nivel={currentLevel}, Items={itemCount}, Entradas={diaryCount}");

        // Resetear diálogos del hub al volver
        Triskel.Dialogue.DialogueZone.ResetearTodosLosDialogos();

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
        if (InventoryData.Instance != null)
        {
            InventoryData.Instance.ClearInventory();
        }
        if (InventoryPersistence.Instance != null)
        {
            InventoryPersistence.Instance.ClearSavedData();
        }

        // 3. Limpiar diario
        if (DiaryManager.Instance != null)
        {
            DiaryManager.Instance.ClearAll();
        }
        if (DiaryPersistence.Instance != null)
        {
            DiaryPersistence.Instance.ClearSavedData();
        }

        // 4. Limpiar PlayerPrefs
        PlayerPrefs.DeleteKey("game_moral");
        PlayerPrefs.DeleteKey("game_level");
        PlayerPrefs.Save();

        // 5. Resetear diálogos del hub
        Triskel.Dialogue.DialogueZone.ResetearTodosLosDialogos();

        Debug.Log("[GameManager] ✓ Nueva partida iniciada");
    }

    /// <summary>
    /// Restaura el estado del juego desde los datos de la API.
    /// </summary>
    /// <param name="gameData">Datos de la partida desde la API REST.</param>
    /// <remarks>
    /// Restaura el nivel actual, reliquias y otros datos relevantes.
    /// Se llama al hacer clic en "Continuar" para cargar una partida existente.
    /// </remarks>
    public void RestoreFromAPI(GameData gameData)
    {
        Debug.Log($"[GameManager] Restaurando estado desde API: {gameData.game_id}");

        // Restaurar nivel actual (convertir nivel API a número de nivel)
        currentLevel = LevelMapper.LevelIndexToAPILevel(1) == gameData.current_level ? 1 :
                       LevelMapper.LevelIndexToAPILevel(2) == gameData.current_level ? 2 :
                       LevelMapper.LevelIndexToAPILevel(3) == gameData.current_level ? 3 :
                       LevelMapper.LevelIndexToAPILevel(4) == gameData.current_level ? 4 : 1;

        // Restaurar reliquias en el inventario
        if (InventoryData.Instance != null && gameData.relics != null)
        {
            InventoryData.Instance.ClearInventory();

            foreach (string relicID in gameData.relics)
            {
                CollectibleItem item = LoadCollectibleItemByID(relicID);
                if (item != null)
                {
                    InventoryData.Instance.AddItem(item);
                    Debug.Log($"[GameManager] Reliquia restaurada: {relicID}");
                }
                else
                {
                    Debug.LogWarning($"[GameManager] No se pudo cargar reliquia: {relicID}");
                }
            }
        }

        // Restaurar moral basado en las decisiones tomadas
        // Calculamos moral basado en si las decisiones fueron buenas o malas
        int calculatedMoral = 0;
        if (gameData.choices != null)
        {
            if (APIConstants.Choices.IsGoodChoice(gameData.choices.senda_ebano))
                calculatedMoral += 1;
            else if (!string.IsNullOrEmpty(gameData.choices.senda_ebano))
                calculatedMoral -= 1;

            if (APIConstants.Choices.IsGoodChoice(gameData.choices.fortaleza_gigantes))
                calculatedMoral += 1;
            else if (!string.IsNullOrEmpty(gameData.choices.fortaleza_gigantes))
                calculatedMoral -= 1;

            if (APIConstants.Choices.IsGoodChoice(gameData.choices.aquelarre_sombras))
                calculatedMoral += 1;
            else if (!string.IsNullOrEmpty(gameData.choices.aquelarre_sombras))
                calculatedMoral -= 1;
        }
        moralScore = calculatedMoral;

        // Reconstruir diario desde datos de la API
        if (DiaryManager.Instance != null)
        {
            int unlockedCount = 0;

            // Entrada del hub: se desbloquea al salir del hub por primera vez
            if (gameData.current_level != APIConstants.Levels.HUB_CENTRAL)
            {
                DiaryManager.Instance.UnlockEntry(0, "intro");
                unlockedCount++;
                Debug.Log("[GameManager] Diario: Entrada hub desbloqueada");
            }

            // Nivel 1: si estamos en nivel 2+, ya completamos nivel 1
            if (currentLevel >= 2)
            {
                string decision;
                if (!string.IsNullOrEmpty(gameData.choices?.senda_ebano))
                {
                    decision = APIConstants.Choices.IsGoodChoice(gameData.choices.senda_ebano)
                        ? "bueno"
                        : "malo";
                    Debug.Log($"[GameManager] Diario: Nivel 1 con decisión guardada '{gameData.choices.senda_ebano}' ({decision})");
                }
                else
                {
                    decision = "bueno"; // Decisión por defecto (buena)
                    Debug.Log("[GameManager] Diario: Nivel 1 sin decisión guardada, usando decisión buena por defecto");
                }
                DiaryManager.Instance.UnlockEntry(1, decision);
                unlockedCount++;
            }

            // Nivel 2: si estamos en nivel 3+, ya completamos nivel 2
            if (currentLevel >= 3)
            {
                string decision;
                if (!string.IsNullOrEmpty(gameData.choices?.fortaleza_gigantes))
                {
                    decision = APIConstants.Choices.IsGoodChoice(gameData.choices.fortaleza_gigantes)
                        ? "bueno"
                        : "malo";
                }
                else
                {
                    decision = "bueno"; // Decisión por defecto (buena)
                }
                DiaryManager.Instance.UnlockEntry(2, decision);
                unlockedCount++;
                Debug.Log($"[GameManager] Diario: Entrada nivel 2 desbloqueada ({decision})");
            }

            // Nivel 3: si estamos en nivel 4, ya completamos nivel 3
            if (currentLevel >= 4)
            {
                string decision;
                if (!string.IsNullOrEmpty(gameData.choices?.aquelarre_sombras))
                {
                    decision = APIConstants.Choices.IsGoodChoice(gameData.choices.aquelarre_sombras)
                        ? "bueno"
                        : "malo";
                }
                else
                {
                    decision = "bueno"; // Decisión por defecto (buena)
                }
                DiaryManager.Instance.UnlockEntry(3, decision);
                unlockedCount++;
                Debug.Log($"[GameManager] Diario: Entrada nivel 3 desbloqueada ({decision})");
            }

            Debug.Log($"[GameManager] Diario reconstruido desde API: {unlockedCount} entradas desbloqueadas");
        }

        Debug.Log($"[GameManager] ✓ Estado restaurado: Nivel={currentLevel}, Moral={moralScore}, Reliquias={gameData.relics?.Length ?? 0}");
    }

    /// <summary>
    /// Carga un CollectibleItem desde Resources usando su ID.
    /// </summary>
    /// <param name="itemID">ID del item ("lirio", "hacha", "manto").</param>
    /// <returns>CollectibleItem cargado, o null si no se encuentra.</returns>
    /// <remarks>
    /// Los CollectibleItem deben estar en la carpeta Resources/Items/ con nombres:
    /// - Lirio.asset (para itemID "lirio")
    /// - Hacha.asset (para itemID "hacha")
    /// - Manto.asset (para itemID "manto")
    /// </remarks>
    private CollectibleItem LoadCollectibleItemByID(string itemID)
    {
        if (string.IsNullOrEmpty(itemID))
            return null;

        // Capitalizar primera letra: "lirio" → "Lirio"
        string capitalizedID = char.ToUpper(itemID[0]) + itemID.Substring(1).ToLower();

        // Cargar desde Resources/Items/
        CollectibleItem item = Resources.Load<CollectibleItem>($"Items/{capitalizedID}");

        if (item == null)
        {
            Debug.LogError($"[GameManager] No se encontró CollectibleItem en Resources/Items/{capitalizedID}");
        }

        return item;
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
    /// Modifica la moral del jugador y registra la decisión en la API.
    /// </summary>
    /// <param name="delta">Cambio en moral (+1, -1, etc.)</param>
    /// <param name="choice">Decisión moral tomada (usar APIConstants.Choices)</param>
    public void ModifyMoralWithChoice(int delta, string choice)
    {
        ModifyMoral(delta);
        apiTracker?.RegisterMoralChoice(choice);
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
        Debug.Log($"Items inventario: {InventoryData.Instance?.GetAllItems().Count ?? 0}");
        Debug.Log($"Entradas diario: {DiaryManager.Instance?.GetUnlockedEntries().Count ?? 0}");
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
    
    /// <summary>
    /// Inicia una transición de nivel con ID dinámico basado en decisiones.
    /// </summary>
    public void IrATransicion(int nivelCompletado, string
        escenaDestino)
    {
        // Guardar automáticamente el inventario y estado al cambiar de nivel
        SaveGame();

        // Determinar el ID de transición según moral/decisiones
        string transitionID = ObtenerIDTransicion(nivelCompletado);

        Debug.Log($"[GameManager] Transición (Autoguardado): {transitionID} → {escenaDestino}");

    // Pasar datos a la escena de transición
    TransitionManager.TransitionID = transitionID;
    TransitionManager.SiguienteEscena = escenaDestino;

    // Cargar escena de transición
    SceneManager.LoadScene("LevelTransition");
}

    /// <summary>
    /// Determina qué texto de transición mostrar según nivel y decisiones.
    /// </summary>
    private string ObtenerIDTransicion(int nivel)
    {
        // Nivel 0 (Hub) no tiene moral, siempre usa el mismo texto
        if (nivel == 0)
        {
            return "nivel0";
        }
        
        // Para otros niveles, basado en moral
        string sufijo = moralScore >= 0 ? "bueno" : "malo";
        return $"nivel{nivel}_{sufijo}";
    }

    /// <summary>
    /// Maneja la muerte del jugador con transición y reinicio de nivel.
    /// </summary>
    public void OnJugadorMuerto()
    {
        Debug.Log("[GameManager] Jugador ha muerto. Reiniciando nivel con transición...");

        // Resetear tracking de nivel (tiempo y muertes del nivel actual)
        apiTracker?.ResetLevelTracking();

        // Eliminar la reliquia del nivel actual (se pierde al morir)
        RemoveCurrentLevelRelic();

        // Determinar el nivel actual
        string escenaActual = SceneManager.GetActiveScene().name;

        // Determinar qué texto de muerte mostrar
        string transitionID = ObtenerIDMuerte(escenaActual);

        // Reiniciar el mismo nivel
        TransitionManager.TransitionID = transitionID;
        TransitionManager.SiguienteEscena = escenaActual; // Misma escena (reiniciar)

        SceneManager.LoadScene("LevelTransition");
    }

    /// <summary>
    /// Obtiene el ID de transición de muerte según la escena actual.
    /// </summary>
    private string ObtenerIDMuerte(string nombreEscena)
    {
        // Detectar en qué nivel murió
        if (nombreEscena.Contains("Cuadrante1"))
            return "muerte_nivel1";
        else if (nombreEscena.Contains("Cuadrante2"))
            return "muerte_nivel2";
        else if (nombreEscena.Contains("Cuadrante3"))
            return "muerte_nivel3";
        else if (nombreEscena.Contains("Cuadrante4"))
            return "muerte_nivel4";
        else if (nombreEscena.Contains("Hub"))
            return "muerte_hub";
        else
            return "muerte_hub"; // Default
    }
}