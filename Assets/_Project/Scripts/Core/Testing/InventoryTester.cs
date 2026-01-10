using UnityEngine;
using UnityEngine.UI;

namespace Triskel.Core
{
    /// <summary>
    /// Tester visual para el sistema de inventario.
    /// Usa un Canvas con botones para probar las funciones del inventario.
    ///
    /// SETUP:
    /// 1. Crea un Canvas en la escena
    /// 2. Añade este script al Canvas
    /// 3. Arrastra los CollectibleItems de prueba al array
    /// 4. Asigna los botones en el Inspector
    /// 5. Los botones llamarán automáticamente a las funciones
    /// </summary>
    public class InventoryTester : MonoBehaviour
    {
        [Header("Items de Prueba")]
        [Tooltip("Arrastra aquí los 3 items de prueba")]
        [SerializeField] private CollectibleItem[] testItems = new CollectibleItem[3];

        [Header("Botones UI")]
        [Tooltip("Arrastra los botones del Canvas aquí")]
        [SerializeField] private Button btnAddItem1;
        [SerializeField] private Button btnAddItem2;
        [SerializeField] private Button btnAddItem3;
        [SerializeField] private Button btnRemoveLast;
        [SerializeField] private Button btnClearAll;
        [SerializeField] private Button btnSave;
        [SerializeField] private Button btnLoad;
        [SerializeField] private Button btnPrint;

        private void Start()
        {
            // Suscribir botones a funciones
            if (btnAddItem1 != null) btnAddItem1.onClick.AddListener(() => AddItem(0));
            if (btnAddItem2 != null) btnAddItem2.onClick.AddListener(() => AddItem(1));
            if (btnAddItem3 != null) btnAddItem3.onClick.AddListener(() => AddItem(2));
            if (btnRemoveLast != null) btnRemoveLast.onClick.AddListener(RemoveLastItem);
            if (btnClearAll != null) btnClearAll.onClick.AddListener(ClearAll);
            if (btnSave != null) btnSave.onClick.AddListener(SaveInventory);
            if (btnLoad != null) btnLoad.onClick.AddListener(LoadInventory);
            if (btnPrint != null) btnPrint.onClick.AddListener(PrintInventory);
        }

        /// <summary>
        /// Añade un item de prueba al inventario.
        /// </summary>
        public void AddItem(int index)
        {
            if (index < 0 || index >= testItems.Length)
            {
                Debug.LogWarning($"[InventoryTester] Índice {index} fuera de rango.");
                return;
            }

            if (testItems[index] == null)
            {
                Debug.LogWarning($"[InventoryTester] No hay item asignado en el índice {index}.");
                return;
            }

            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryTester] InventoryData no está en la escena.");
                return;
            }

            bool success = InventoryData.Instance.AddItem(testItems[index]);

            if (success)
            {
                Debug.Log($"<color=green>[TEST] ✓ Item añadido: {testItems[index].displayName}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[TEST] ⚠ No se pudo añadir: {testItems[index].displayName}</color>");
            }
        }

        /// <summary>
        /// Remueve el último item del inventario.
        /// </summary>
        public void RemoveLastItem()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryTester] InventoryData no está en la escena.");
                return;
            }

            var items = InventoryData.Instance.GetAllItems();
            if (items.Count == 0)
            {
                Debug.LogWarning("<color=yellow>[TEST] El inventario está vacío.</color>");
                return;
            }

            CollectibleItem lastItem = items[items.Count - 1];
            bool success = InventoryData.Instance.RemoveItem(lastItem.itemID);

            if (success)
            {
                Debug.Log($"<color=green>[TEST] ✓ Item removido: {lastItem.displayName}</color>");
            }
        }

        /// <summary>
        /// Limpia todo el inventario.
        /// </summary>
        public void ClearAll()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryTester] InventoryData no está en la escena.");
                return;
            }

            InventoryData.Instance.ClearInventory();
            Debug.Log("<color=cyan>[TEST] 🗑 Inventario limpiado.</color>");
        }

        /// <summary>
        /// Guarda el inventario.
        /// </summary>
        public void SaveInventory()
        {
            if (InventoryPersistence.Instance == null)
            {
                Debug.LogError("[InventoryTester] InventoryPersistence no está en la escena.");
                return;
            }

            InventoryPersistence.Instance.SaveInventory();
            Debug.Log("<color=cyan>[TEST] 💾 Inventario guardado.</color>");
        }

        /// <summary>
        /// Carga el inventario.
        /// </summary>
        public void LoadInventory()
        {
            if (InventoryPersistence.Instance == null)
            {
                Debug.LogError("[InventoryTester] InventoryPersistence no está en la escena.");
                return;
            }

            InventoryPersistence.Instance.LoadInventory();
            Debug.Log("<color=cyan>[TEST] 📂 Inventario cargado.</color>");
        }

        /// <summary>
        /// Imprime el estado del inventario en consola.
        /// </summary>
        public void PrintInventory()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[InventoryTester] InventoryData no está en la escena.");
                return;
            }

            var items = InventoryData.Instance.GetAllItems();

            Debug.Log($"<color=white>═══════════════════════════════</color>");
            Debug.Log($"<color=cyan>[TEST] 📋 INVENTARIO ({items.Count}/3)</color>");
            Debug.Log($"<color=white>═══════════════════════════════</color>");

            if (items.Count == 0)
            {
                Debug.Log("<color=grey>  (vacío)</color>");
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Debug.Log($"<color=white>  Slot {i + 1}: <b>{items[i].displayName}</b> (ID: {items[i].itemID})</color>");
                }
            }

            Debug.Log($"<color=white>═══════════════════════════════</color>");
        }

        private void OnDestroy()
        {
            // Limpiar suscripciones
            if (btnAddItem1 != null) btnAddItem1.onClick.RemoveAllListeners();
            if (btnAddItem2 != null) btnAddItem2.onClick.RemoveAllListeners();
            if (btnAddItem3 != null) btnAddItem3.onClick.RemoveAllListeners();
            if (btnRemoveLast != null) btnRemoveLast.onClick.RemoveAllListeners();
            if (btnClearAll != null) btnClearAll.onClick.RemoveAllListeners();
            if (btnSave != null) btnSave.onClick.RemoveAllListeners();
            if (btnLoad != null) btnLoad.onClick.RemoveAllListeners();
            if (btnPrint != null) btnPrint.onClick.RemoveAllListeners();
        }
    }
}
