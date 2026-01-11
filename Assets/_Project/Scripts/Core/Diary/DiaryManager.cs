using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor central del sistema de diario narrativo (Singleton).
    ///
    /// RESPONSABILIDADES:
    /// - Recibir notificación de "nivel completado" con condiciones.
    /// - Elegir la DiaryEntryData correcta según el nivel y condiciones.
    /// - Guardar el entryID seleccionado.
    /// - Exponer lista ordenada de entradas desbloqueadas.
    /// - Permitir consulta del texto asociado a un entryID.
    ///
    /// NO DEBE:
    /// - Tener referencias a UI.
    /// - Tener textos hardcodeados.
    /// </summary>
    public class DiaryManager : MonoBehaviour
    {
        // Singleton instance
        public static DiaryManager Instance { get; private set; }

        [Header("Referencias")]
        [Tooltip("Arrastra aquí todas las DiaryEntryData assets creadas")]
        [SerializeField] private DiaryEntryData[] allEntries;

        // Diccionario para búsqueda rápida por entryID
        private Dictionary<string, DiaryEntryData> entryDictionary;

        // Lista de entryIDs desbloqueados (en orden cronológico)
        private List<string> unlockedEntryIDs = new List<string>();

        private void Awake()
        {
            // Implementación Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Construir diccionario de entradas disponibles
            BuildEntryDictionary();
        }

        private void Start()
        {
            // Cargar entradas desbloqueadas al iniciar
            LoadUnlockedEntries();
        }

        /// <summary>
        /// Construye un diccionario de entradas para búsqueda rápida.
        /// </summary>
        private void BuildEntryDictionary()
        {
            entryDictionary = new Dictionary<string, DiaryEntryData>();

            foreach (var entry in allEntries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.entryID))
                {
                    if (entryDictionary.ContainsKey(entry.entryID))
                    {
                        Debug.LogError($"[DiaryManager] ¡ID duplicado detectado! '{entry.entryID}' ya existe. Revisa tus DiaryEntryData assets.");
                        continue;
                    }

                    entryDictionary[entry.entryID] = entry;
                }
                else
                {
                    Debug.LogWarning($"[DiaryManager] DiaryEntryData con entryID vacío detectado: {(entry != null ? entry.name : "null")}");
                }
            }

            Debug.Log($"[DiaryManager] Entradas disponibles registradas: {entryDictionary.Count}");
        }

        #region Public Methods - Level Completion

        /// <summary>
        /// Notifica al diario que un nivel ha sido completado.
        /// Selecciona y desbloquea automáticamente la entrada correspondiente.
        /// </summary>
        /// <param name="levelIndex">Índice del nivel completado (0 = Nivel 1)</param>
        /// <param name="conditionIDs">Lista de IDs de condiciones cumplidas durante el nivel</param>
        public void OnLevelCompleted(int levelIndex, string[] conditionIDs)
        {
            if (conditionIDs == null || conditionIDs.Length == 0)
            {
                Debug.LogWarning($"[DiaryManager] Nivel {levelIndex} completado sin condiciones. No se desbloqueará ninguna entrada.");
                return;
            }

            // Buscar la entrada que coincida con el nivel y alguna de las condiciones
            DiaryEntryData matchedEntry = FindEntryForLevel(levelIndex, conditionIDs);

            if (matchedEntry != null)
            {
                UnlockEntry(matchedEntry.entryID);
            }
            else
            {
                Debug.LogWarning($"[DiaryManager] No se encontró entrada para nivel {levelIndex} con condiciones: {string.Join(", ", conditionIDs)}");
            }
        }

        /// <summary>
        /// Busca la entrada correcta para un nivel dado y sus condiciones.
        /// Prioridad: Primera coincidencia encontrada.
        /// </summary>
        private DiaryEntryData FindEntryForLevel(int levelIndex, string[] conditionIDs)
        {
            // Filtrar entradas por nivel
            var entriesForLevel = allEntries.Where(e => e != null && e.levelIndex == levelIndex).ToList();

            if (entriesForLevel.Count == 0)
            {
                Debug.LogWarning($"[DiaryManager] No hay entradas configuradas para el nivel {levelIndex}.");
                return null;
            }

            // Buscar coincidencia con condiciones (primera que coincida)
            foreach (var conditionID in conditionIDs)
            {
                var matchedEntry = entriesForLevel.FirstOrDefault(e => e.conditionID == conditionID);
                if (matchedEntry != null)
                {
                    Debug.Log($"[DiaryManager] Entrada encontrada: '{matchedEntry.entryID}' para nivel {levelIndex} y condición '{conditionID}'");
                    return matchedEntry;
                }
            }

            // Si no hay coincidencia exacta, retornar la primera entrada del nivel como fallback
            Debug.LogWarning($"[DiaryManager] No se encontró coincidencia exacta. Usando entrada por defecto para nivel {levelIndex}.");
            return entriesForLevel.First();
        }

        #endregion

        #region Public Methods - Entry Management

        /// <summary>
        /// Desbloquea una entrada del diario.
        /// </summary>
        /// <param name="entryID">ID de la entrada a desbloquear</param>
        public void UnlockEntry(string entryID)
        {
            if (string.IsNullOrEmpty(entryID))
            {
                Debug.LogError("[DiaryManager] No se puede desbloquear entrada con ID vacío.");
                return;
            }

            // Verificar que la entrada existe
            if (!entryDictionary.ContainsKey(entryID))
            {
                Debug.LogError($"[DiaryManager] No se encontró entrada con ID '{entryID}'.");
                return;
            }

            // Verificar si ya está desbloqueada
            if (unlockedEntryIDs.Contains(entryID))
            {
                Debug.LogWarning($"[DiaryManager] Entrada '{entryID}' ya está desbloqueada.");
                return;
            }

            // Desbloquear
            unlockedEntryIDs.Add(entryID);
            Debug.Log($"[DiaryManager] ✓ Entrada desbloqueada: '{entryID}'");

            // Guardar automáticamente
            SaveUnlockedEntries();
        }

        /// <summary>
        /// Verifica si una entrada está desbloqueada.
        /// </summary>
        public bool IsEntryUnlocked(string entryID)
        {
            return unlockedEntryIDs.Contains(entryID);
        }

        /// <summary>
        /// Obtiene la lista de IDs de entradas desbloqueadas (en orden cronológico).
        /// </summary>
        public string[] GetUnlockedEntryIDs()
        {
            return unlockedEntryIDs.ToArray();
        }

        /// <summary>
        /// Obtiene los datos completos de una entrada por su ID.
        /// </summary>
        /// <returns>DiaryEntryData o null si no existe</returns>
        public DiaryEntryData GetEntryData(string entryID)
        {
            if (entryDictionary.TryGetValue(entryID, out DiaryEntryData entry))
            {
                return entry;
            }

            Debug.LogWarning($"[DiaryManager] No se encontró entrada con ID '{entryID}'.");
            return null;
        }

        /// <summary>
        /// Obtiene todas las entradas desbloqueadas ordenadas cronológicamente.
        /// </summary>
        public List<DiaryEntryData> GetUnlockedEntries()
        {
            List<DiaryEntryData> unlockedEntries = new List<DiaryEntryData>();

            foreach (string entryID in unlockedEntryIDs)
            {
                DiaryEntryData entry = GetEntryData(entryID);
                if (entry != null)
                {
                    unlockedEntries.Add(entry);
                }
            }

            return unlockedEntries;
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Guarda las entradas desbloqueadas.
        /// </summary>
        private void SaveUnlockedEntries()
        {
            if (DiaryPersistence.Instance != null)
            {
                DiaryPersistence.Instance.SaveUnlockedEntries(unlockedEntryIDs.ToArray());
            }
            else
            {
                Debug.LogError("[DiaryManager] DiaryPersistence no está disponible.");
            }
        }

        /// <summary>
        /// Carga las entradas desbloqueadas.
        /// </summary>
        private void LoadUnlockedEntries()
        {
            if (DiaryPersistence.Instance != null)
            {
                string[] loadedIDs = DiaryPersistence.Instance.LoadUnlockedEntries();

                // Limpiar y cargar
                unlockedEntryIDs.Clear();
                unlockedEntryIDs.AddRange(loadedIDs);

                Debug.Log($"[DiaryManager] Entradas cargadas: {unlockedEntryIDs.Count}");
            }
            else
            {
                Debug.LogError("[DiaryManager] DiaryPersistence no está disponible.");
            }
        }

        /// <summary>
        /// Limpia todas las entradas desbloqueadas (para reset/debug).
        /// </summary>
        public void ClearAllEntries()
        {
            unlockedEntryIDs.Clear();

            if (DiaryPersistence.Instance != null)
            {
                DiaryPersistence.Instance.ClearSavedData();
            }

            Debug.Log("[DiaryManager] Todas las entradas han sido limpiadas.");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug: Desbloquear Entrada de Prueba")]
        public void DebugUnlockTestEntry()
        {
            if (allEntries.Length > 0)
            {
                UnlockEntry(allEntries[0].entryID);
            }
            else
            {
                Debug.LogWarning("[DiaryManager] No hay entradas configuradas para desbloquear.");
            }
        }

        [ContextMenu("Debug: Mostrar Entradas Desbloqueadas")]
        public void DebugShowUnlockedEntries()
        {
            Debug.Log($"[DiaryManager] Entradas desbloqueadas ({unlockedEntryIDs.Count}):");
            foreach (string entryID in unlockedEntryIDs)
            {
                DiaryEntryData entry = GetEntryData(entryID);
                if (entry != null)
                {
                    Debug.Log($" - {entryID}: '{entry.title}' (Nivel {entry.levelIndex})");
                }
            }
        }

        [ContextMenu("Debug: Limpiar Todas las Entradas")]
        public void DebugClearEntries()
        {
            ClearAllEntries();
        }

        [ContextMenu("Debug: Simular Nivel Completado")]
        public void DebugSimulateLevelCompleted()
        {
            // Simular nivel 0 con condición de prueba
            string[] testConditions = new string[] { "test_condition" };
            OnLevelCompleted(0, testConditions);
        }

        #endregion
    }
}
