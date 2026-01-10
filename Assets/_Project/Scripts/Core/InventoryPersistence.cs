using System.Collections.Generic;
using UnityEngine;
using Triskel.API;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor de persistencia del inventario.
    ///
    /// TOGGLE: Cambia entre guardado local (PlayerPrefs) y remoto (API).
    ///
    /// ┌──────────────────────────────────────────┐
    /// │  useAPI = false  →  PlayerPrefs (Local)  │
    /// │  useAPI = true   →  TriskelAPI (Remote)  │
    /// └──────────────────────────────────────────┘
    ///
    /// RESPONSABILIDAD: Guardar y cargar el inventario, NO gestionar lógica.
    /// </summary>
    public class InventoryPersistence : MonoBehaviour
    {
        [Header("🔧 TOGGLE: Modo de Persistencia")]
        [Tooltip("FALSE = PlayerPrefs local | TRUE = API remota")]
        [SerializeField] private bool useAPI = false;

        [Header("Referencias")]
        [Tooltip("Arrastra aquí los 3 CollectibleItem assets (Lavender, Lily, etc.)")]
        [SerializeField] private CollectibleItem[] availableItems;

        // Diccionario para búsqueda rápida por ID
        private Dictionary<string, CollectibleItem> itemDictionary;

        // PlayerPrefs keys
        private const string PREFS_KEY_RELICS = "inventory_relics";

        private void Awake()
        {
            // Construir diccionario de items disponibles
            BuildItemDictionary();
        }

        /// <summary>
        /// Construye un diccionario de items para búsqueda rápida.
        /// </summary>
        private void BuildItemDictionary()
        {
            itemDictionary = new Dictionary<string, CollectibleItem>();

            foreach (var item in availableItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemID))
                {
                    itemDictionary[item.itemID] = item;
                }
            }

            Debug.Log($"[InventoryPersistence] Items disponibles registrados: {itemDictionary.Count}");
        }

        #region Public Methods

        /// <summary>
        /// Guarda el inventario actual.
        /// Elige automáticamente entre local o API según el toggle.
        /// </summary>
        public void SaveInventory()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryPersistence] InventoryData no está inicializado.");
                return;
            }

            string[] relics = InventoryData.Instance.GetItemIDs();

            if (useAPI)
            {
                SaveToAPI(relics);
            }
            else
            {
                SaveToPlayerPrefs(relics);
            }
        }

        /// <summary>
        /// Carga el inventario guardado.
        /// Elige automáticamente entre local o API según el toggle.
        /// </summary>
        public void LoadInventory()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryPersistence] InventoryData no está inicializado.");
                return;
            }

            if (useAPI)
            {
                LoadFromAPI();
            }
            else
            {
                LoadFromPlayerPrefs();
            }
        }

        /// <summary>
        /// Limpia todos los datos guardados (local y/o API).
        /// </summary>
        public void ClearSavedData()
        {
            // Limpiar local
            PlayerPrefs.DeleteKey(PREFS_KEY_RELICS);
            PlayerPrefs.Save();

            // Limpiar memoria
            if (InventoryData.Instance != null)
            {
                InventoryData.Instance.ClearInventory();
            }

            Debug.Log("[InventoryPersistence] Datos guardados limpiados.");
        }

        #endregion

        #region Local Storage (PlayerPrefs)

        /// <summary>
        /// Guarda el inventario en PlayerPrefs (guardado local).
        /// Formato: "lirio,hacha,manto"
        /// </summary>
        private void SaveToPlayerPrefs(string[] relics)
        {
            string csv = string.Join(",", relics);
            PlayerPrefs.SetString(PREFS_KEY_RELICS, csv);
            PlayerPrefs.Save();

            Debug.Log($"[InventoryPersistence] ✓ Guardado LOCAL: {csv}");
        }

        /// <summary>
        /// Carga el inventario desde PlayerPrefs (guardado local).
        /// </summary>
        private void LoadFromPlayerPrefs()
        {
            string csv = PlayerPrefs.GetString(PREFS_KEY_RELICS, "");

            if (string.IsNullOrEmpty(csv))
            {
                Debug.Log("[InventoryPersistence] No hay datos locales guardados.");
                return;
            }

            string[] relicIDs = csv.Split(',');
            LoadRelicsIntoInventory(relicIDs);

            Debug.Log($"[InventoryPersistence] ✓ Cargado LOCAL: {csv}");
        }

        #endregion

        #region API Storage (TriskelAPI)

        /// <summary>
        /// Guarda el inventario en la API remota.
        /// Endpoint: PATCH /v1/games/{game_id}
        /// </summary>
        private void SaveToAPI(string[] relics)
        {
            if (TriskelAPIClient.Instance == null)
            {
                Debug.LogError("[InventoryPersistence] TriskelAPIClient no está inicializado.");
                return;
            }

            TriskelAPIClient.Instance.UpdateGameRelics(
                relics,
                onSuccess: () =>
                {
                    Debug.Log($"[InventoryPersistence] ✓ Guardado API: {string.Join(", ", relics)}");
                },
                onError: (error) =>
                {
                    Debug.LogError($"[InventoryPersistence] ✗ Error al guardar en API: {error}");
                    // Fallback: Guardar local si falla la API
                    Debug.Log("[InventoryPersistence] Guardando en local como fallback...");
                    SaveToPlayerPrefs(relics);
                }
            );
        }

        /// <summary>
        /// Carga el inventario desde la API remota.
        /// Endpoint: GET /v1/games/{game_id}
        /// </summary>
        private void LoadFromAPI()
        {
            if (TriskelAPIClient.Instance == null)
            {
                Debug.LogError("[InventoryPersistence] TriskelAPIClient no está inicializado.");
                return;
            }

            string gameID = TriskelAPIClient.Instance.currentGameID;

            if (string.IsNullOrEmpty(gameID))
            {
                Debug.LogWarning("[InventoryPersistence] No hay gameID configurado. No se puede cargar desde API.");
                return;
            }

            TriskelAPIClient.Instance.GetGame(
                gameID,
                onSuccess: (gameData) =>
                {
                    if (gameData.relics != null && gameData.relics.Length > 0)
                    {
                        LoadRelicsIntoInventory(gameData.relics);
                        Debug.Log($"[InventoryPersistence] ✓ Cargado API: {string.Join(", ", gameData.relics)}");
                    }
                    else
                    {
                        Debug.Log("[InventoryPersistence] La partida no tiene reliquias guardadas.");
                    }
                },
                onError: (error) =>
                {
                    Debug.LogError($"[InventoryPersistence] ✗ Error al cargar desde API: {error}");
                    // Fallback: Intentar cargar local
                    Debug.Log("[InventoryPersistence] Intentando cargar desde local como fallback...");
                    LoadFromPlayerPrefs();
                }
            );
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Carga reliquias en InventoryData desde un array de IDs.
        /// </summary>
        private void LoadRelicsIntoInventory(string[] relicIDs)
        {
            if (InventoryData.Instance == null) return;

            // Limpiar inventario antes de cargar
            InventoryData.Instance.ClearInventory();

            // Cargar items
            foreach (string id in relicIDs)
            {
                if (string.IsNullOrEmpty(id)) continue;

                if (itemDictionary.TryGetValue(id, out CollectibleItem item))
                {
                    InventoryData.Instance.AddItem(item);
                }
                else
                {
                    Debug.LogError($"[InventoryPersistence] No se encontró item con ID '{id}' en availableItems.");
                }
            }
        }

        /// <summary>
        /// Obtiene un CollectibleItem por su ID.
        /// </summary>
        public CollectibleItem GetItemByID(string itemID)
        {
            if (itemDictionary.TryGetValue(itemID, out CollectibleItem item))
            {
                return item;
            }

            Debug.LogWarning($"[InventoryPersistence] Item con ID '{itemID}' no encontrado.");
            return null;
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug: Guardar Inventario")]
        public void DebugSave()
        {
            SaveInventory();
        }

        [ContextMenu("Debug: Cargar Inventario")]
        public void DebugLoad()
        {
            LoadInventory();
        }

        [ContextMenu("Debug: Limpiar Datos Guardados")]
        public void DebugClear()
        {
            ClearSavedData();
        }

        #endregion
    }
}
