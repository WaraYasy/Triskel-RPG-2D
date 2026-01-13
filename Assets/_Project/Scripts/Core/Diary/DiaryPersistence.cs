using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor de persistencia del diario (Singleton).
    /// Guarda y carga las entradas desbloqueadas usando PlayerPrefs (guardado local).
    ///
    /// RESPONSABILIDAD: Guardar y cargar el array de entryIDs, NO gestionar lógica narrativa.
    /// </summary>
    public class DiaryPersistence : MonoBehaviour
    {
        // Singleton instance
        public static DiaryPersistence Instance { get; private set; }

        // PlayerPrefs key
        private const string PREFS_KEY_ENTRIES = "diary_unlocked_entries";

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
        }

        #region Public Methods

        /// <summary>
        /// Guarda las entradas desbloqueadas en PlayerPrefs.
        /// </summary>
        /// <param name="entryIDs">Array de IDs de entradas desbloqueadas</param>
        public void SaveUnlockedEntries(string[] entryIDs)
        {
            SaveToPlayerPrefs(entryIDs);
        }

        /// <summary>
        /// Carga las entradas desbloqueadas desde PlayerPrefs.
        /// </summary>
        /// <returns>Array de IDs de entradas desbloqueadas</returns>
        public string[] LoadUnlockedEntries()
        {
            return LoadFromPlayerPrefs();
        }

        /// <summary>
        /// Limpia todos los datos guardados del diario.
        /// </summary>
        public void ClearSavedData()
        {
            PlayerPrefs.DeleteKey(PREFS_KEY_ENTRIES);
            PlayerPrefs.Save();

            Debug.Log("[DiaryPersistence] Datos del diario limpiados.");
        }

        #endregion

        #region Local Storage (PlayerPrefs)

        /// <summary>
        /// Guarda las entradas en PlayerPrefs (guardado local).
        /// Formato: "level1_saved_child,level2_ignored_npc,level3_completed"
        /// </summary>
        private void SaveToPlayerPrefs(string[] entryIDs)
        {
            string csv = string.Join(",", entryIDs);
            PlayerPrefs.SetString(PREFS_KEY_ENTRIES, csv);
            PlayerPrefs.Save();

            Debug.Log($"[DiaryPersistence] ✓ Guardado LOCAL: {csv}");
        }

        /// <summary>
        /// Carga las entradas desde PlayerPrefs (guardado local).
        /// </summary>
        private string[] LoadFromPlayerPrefs()
        {
            string csv = PlayerPrefs.GetString(PREFS_KEY_ENTRIES, "");

            if (string.IsNullOrEmpty(csv))
            {
                Debug.Log("[DiaryPersistence] No hay datos locales guardados para el diario.");
                return new string[0];
            }

            string[] entryIDs = csv.Split(',');

            Debug.Log($"[DiaryPersistence] ✓ Cargado LOCAL: {csv}");

            return entryIDs;
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug: Guardar Diario")]
        public void DebugSave()
        {
            // Guardar datos ficticios para prueba
            string[] testEntries = new string[] { "level1_test", "level2_test" };
            SaveUnlockedEntries(testEntries);
        }

        [ContextMenu("Debug: Cargar Diario")]
        public void DebugLoad()
        {
            string[] entries = LoadUnlockedEntries();
            Debug.Log($"[DiaryPersistence] Entradas cargadas: {string.Join(", ", entries)}");
        }

        [ContextMenu("Debug: Limpiar Datos Guardados")]
        public void DebugClear()
        {
            ClearSavedData();
        }

        #endregion
    }
}
