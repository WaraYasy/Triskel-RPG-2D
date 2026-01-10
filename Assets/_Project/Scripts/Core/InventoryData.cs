using System;
using System.Collections.Generic;
using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor central del inventario del jugador (Singleton).
    /// Mantiene el estado en memoria de qué items tiene el jugador.
    ///
    /// RESPONSABILIDAD: Solo gestiona datos, NO persistencia ni UI.
    /// </summary>
    public class InventoryData : MonoBehaviour
    {
        // Singleton instance
        public static InventoryData Instance { get; private set; }

        [Header("Configuración")]
        [Tooltip("Máximo de items que puede tener el jugador (3 reliquias)")]
        [SerializeField] private int maxInventorySize = 3;

        // Estado actual del inventario
        private List<CollectibleItem> collectedItems = new List<CollectibleItem>();

        // Evento que se dispara cuando cambia el inventario
        public event Action<CollectibleItem> OnItemAdded;
        public event Action<CollectibleItem> OnItemRemoved;
        public event Action OnInventoryCleared;

        #region Unity Lifecycle

        private void Awake()
        {
            // Implementación Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Añade un item al inventario.
        /// </summary>
        /// <param name="item">Item a añadir</param>
        /// <returns>True si se añadió correctamente, False si no</returns>
        public bool AddItem(CollectibleItem item)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventoryData] Intentaste añadir un item null.");
                return false;
            }

            // Validar capacidad máxima
            if (collectedItems.Count >= maxInventorySize)
            {
                Debug.LogWarning($"[InventoryData] Inventario lleno ({maxInventorySize}/{maxInventorySize}). No se puede añadir '{item.displayName}'.");
                return false;
            }

            // Validar duplicados (no permitir la misma reliquia dos veces)
            if (HasItem(item.itemID))
            {
                Debug.LogWarning($"[InventoryData] Ya tienes '{item.displayName}' en el inventario.");
                return false;
            }

            // Añadir item
            collectedItems.Add(item);
            Debug.Log($"[InventoryData] Item añadido: {item.displayName} ({collectedItems.Count}/{maxInventorySize})");

            // Disparar evento
            OnItemAdded?.Invoke(item);

            return true;
        }

        /// <summary>
        /// Remueve un item del inventario.
        /// </summary>
        /// <param name="itemID">ID del item a remover</param>
        /// <returns>True si se removió, False si no existía</returns>
        public bool RemoveItem(string itemID)
        {
            CollectibleItem itemToRemove = collectedItems.Find(i => i.itemID == itemID);

            if (itemToRemove == null)
            {
                Debug.LogWarning($"[InventoryData] No se encontró item con ID '{itemID}' para remover.");
                return false;
            }

            collectedItems.Remove(itemToRemove);
            Debug.Log($"[InventoryData] Item removido: {itemToRemove.displayName}");

            // Disparar evento
            OnItemRemoved?.Invoke(itemToRemove);

            return true;
        }

        /// <summary>
        /// Verifica si el jugador tiene un item específico.
        /// </summary>
        public bool HasItem(string itemID)
        {
            return collectedItems.Exists(i => i.itemID == itemID);
        }

        /// <summary>
        /// Obtiene el item en un slot específico (0, 1, 2).
        /// </summary>
        /// <returns>Item en el slot, o null si está vacío</returns>
        public CollectibleItem GetItemAtSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= collectedItems.Count)
            {
                return null;
            }

            return collectedItems[slotIndex];
        }

        /// <summary>
        /// Obtiene todos los items del inventario.
        /// </summary>
        public List<CollectibleItem> GetAllItems()
        {
            return new List<CollectibleItem>(collectedItems); // Copia defensiva
        }

        /// <summary>
        /// Obtiene la lista de IDs de items (para enviar a la API).
        /// </summary>
        /// <returns>Array de strings: ["lirio", "hacha", "manto"]</returns>
        public string[] GetItemIDs()
        {
            string[] ids = new string[collectedItems.Count];
            for (int i = 0; i < collectedItems.Count; i++)
            {
                ids[i] = collectedItems[i].itemID;
            }
            return ids;
        }

        /// <summary>
        /// Limpia todo el inventario (al empezar nueva partida).
        /// </summary>
        public void ClearInventory()
        {
            collectedItems.Clear();
            Debug.Log("[InventoryData] Inventario limpiado.");

            // Disparar evento
            OnInventoryCleared?.Invoke();
        }

        /// <summary>
        /// Carga items desde una lista de IDs (usado al cargar partida).
        /// Requiere acceso a la lista de CollectibleItems disponibles.
        /// </summary>
        /// <param name="itemIDs">Array de IDs: ["lirio", "hacha"]</param>
        /// <param name="availableItems">Diccionario de items disponibles</param>
        public void LoadFromIDs(string[] itemIDs, Dictionary<string, CollectibleItem> availableItems)
        {
            ClearInventory();

            foreach (string id in itemIDs)
            {
                if (availableItems.TryGetValue(id, out CollectibleItem item))
                {
                    AddItem(item);
                }
                else
                {
                    Debug.LogError($"[InventoryData] No se encontró CollectibleItem con ID '{id}' en availableItems.");
                }
            }
        }

        /// <summary>
        /// Obtiene el número de items en el inventario.
        /// </summary>
        public int GetItemCount()
        {
            return collectedItems.Count;
        }

        /// <summary>
        /// Verifica si el inventario está lleno.
        /// </summary>
        public bool IsFull()
        {
            return collectedItems.Count >= maxInventorySize;
        }

        /// <summary>
        /// Verifica si el inventario está vacío.
        /// </summary>
        public bool IsEmpty()
        {
            return collectedItems.Count == 0;
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Imprime el estado actual del inventario (solo para debugging).
        /// </summary>
        [ContextMenu("Debug: Print Inventory")]
        public void DebugPrintInventory()
        {
            Debug.Log($"=== INVENTARIO ({collectedItems.Count}/{maxInventorySize}) ===");
            for (int i = 0; i < collectedItems.Count; i++)
            {
                Debug.Log($"  Slot {i}: {collectedItems[i].displayName} (ID: {collectedItems[i].itemID})");
            }
        }

        #endregion
    }
}
