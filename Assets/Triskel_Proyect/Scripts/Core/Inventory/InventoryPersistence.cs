using System.Collections.Generic;
using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor de persistencia del inventario (Singleton).
    /// Guarda y carga el inventario usando PlayerPrefs (guardado local).
    ///
    /// RESPONSABILIDAD: Guardar y cargar el inventario, NO gestionar lógica.
    /// </summary>
    public class InventoryPersistence : MonoBehaviour
    {
        // Singleton instance
        public static InventoryPersistence Instance { get; private set; }

        [Header("Referencias")]
        [Tooltip("Arrastra aquí los 3 CollectibleItem assets (Lavender, Lily, etc.)")]
        [SerializeField] private CollectibleItem[] availableItems;

        // Diccionario para búsqueda rápida por ID
        private Dictionary<string, CollectibleItem> itemDictionary;

        // PlayerPrefs keys
        private const string PREFS_KEY_RELICS = "inventory_relics";

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
        /// Guarda el inventario actual en PlayerPrefs.
        /// </summary>
        public void SaveInventory()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryPersistence] InventoryData no está inicializado.");
                return;
            }

            string[] relics = InventoryData.Instance.GetItemIDs();
            SaveToPlayerPrefs(relics);
        }

        /// <summary>
        /// Carga el inventario guardado desde PlayerPrefs.
        /// </summary>
        public void LoadInventory()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryPersistence] InventoryData no está inicializado.");
                return;
            }

            LoadFromPlayerPrefs();
        }

        /// <summary>
        /// Limpia todos los datos guardados localmente.
        /// </summary>
        public void ClearSavedData()
        {
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
                    // Fallback: Intentar cargar desde Resources si no está en availableItems
                    Debug.LogWarning($"[InventoryPersistence] ID '{id}' no encontrado en availableItems. Buscando en Resources...");
                    
                    string capitalizedID = char.ToUpper(id[0]) + id.Substring(1).ToLower();
                    item = Resources.Load<CollectibleItem>($"Items/{capitalizedID}");
                    
                    if (item != null)
                    {
                        InventoryData.Instance.AddItem(item);
                        Debug.Log($"[InventoryPersistence] ✓ Item cargado desde Resources: {item.displayName}");
                    }
                    else
                    {
                        Debug.LogError($"[InventoryPersistence] No se encontró item con ID '{id}' ni en dictionary ni en Resources.");
                    }
                }
            }
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
