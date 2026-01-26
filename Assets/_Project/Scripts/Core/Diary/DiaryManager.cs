using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor del sistema de diario (Singleton).
    /// Carga entradas desde JSON y desbloquea según decisiones del jugador.
    /// NOTA: La persistencia ahora es manejada por GameManager + DiaryPersistence.
    /// </summary>
    public class DiaryManager : MonoBehaviour
    {
        public static DiaryManager Instance { get; private set; }

        [SerializeField] private string jsonFileName = "DiaryEntries";

        private Dictionary<string, DiaryEntry> entries = new Dictionary<string, DiaryEntry>();
        private List<string> unlockedIDs = new List<string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Si no es raíz, usar el GameObject raíz para DontDestroyOnLoad
            GameObject rootObject = transform.root.gameObject;
            DontDestroyOnLoad(rootObject);

            LoadEntriesFromJSON();
        }

        private void LoadEntriesFromJSON()
        {
            TextAsset json = Resources.Load<TextAsset>($"DiaryData/{jsonFileName}");
            if (json == null)
            {
                Debug.LogError($"[Diario] No se encontró {jsonFileName}.json");
                return;
            }

            DiaryData data = JsonUtility.FromJson<DiaryData>(json.text);
            foreach (var entry in data.entries)
            {
                entries[entry.id] = entry;
            }

            Debug.Log($"[Diario] {entries.Count} entradas cargadas desde JSON");
        }

        /// <summary>
        /// Desbloquea entrada según nivel y decisión.
        /// NOTA: Ya no guarda automáticamente. GameManager se encarga del guardado.
        /// </summary>
        public void UnlockEntry(int level, string decision)
        {
            string id = $"level{level}_{decision}";

            if (!entries.ContainsKey(id))
            {
                Debug.LogWarning($"[Diario] Entrada '{id}' no existe");
                return;
            }

            if (unlockedIDs.Contains(id))
            {
                Debug.Log($"[Diario] '{id}' ya desbloqueada");
                return;
            }

            unlockedIDs.Add(id);
            Debug.Log($"[Diario] ✓ '{id}' desbloqueada");
        }

        /// <summary>
        /// Obtiene entrada por ID.
        /// </summary>
        public DiaryEntry GetEntry(string id)
        {
            return entries.ContainsKey(id) ? entries[id] : null;
        }

        /// <summary>
        /// Obtiene todas las entradas desbloqueadas.
        /// </summary>
        public List<DiaryEntry> GetUnlockedEntries()
        {
            return unlockedIDs.Select(id => GetEntry(id)).Where(e => e != null).ToList();
        }

        /// <summary>
        /// Limpia todas las entradas desbloqueadas.
        /// NOTA: Ya no elimina de PlayerPrefs. GameManager se encarga de la persistencia.
        /// </summary>
        public void ClearAll()
        {
            unlockedIDs.Clear();
            Debug.Log("[Diario] Entradas limpiadas");
        }

        /// <summary>
        /// Restaura una entrada desbloqueada sin guardar (usado al cargar partida desde GameManager).
        /// </summary>
        public void RestoreEntry(string id)
        {
            if (!entries.ContainsKey(id))
            {
                Debug.LogWarning($"[Diario] Entrada '{id}' no existe al restaurar");
                return;
            }

            if (!unlockedIDs.Contains(id))
            {
                unlockedIDs.Add(id);
            }
        }
    }
}
