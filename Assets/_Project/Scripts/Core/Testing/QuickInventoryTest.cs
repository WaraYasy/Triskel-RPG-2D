using UnityEngine;
using Triskel.Core;

namespace Triskel.Testing
{
    /// <summary>
    /// Script de prueba SIMPLE para el sistema de inventario (sin botones).
    /// TECLAS:
    /// - Q/W/E = Añadir items 1/2/3
    /// - R = Remover último item
    /// - T = Limpiar inventario
    /// - Y = Guardar
    /// - U = Cargar
    /// - I = Imprimir estado
    /// </summary>
    public class QuickInventoryTest : MonoBehaviour
    {
        [Header("Items de Prueba")]
        [Tooltip("Arrastra aquí los 3 CollectibleItem assets desde Resources/Items")]
        [SerializeField] private CollectibleItem[] testItems = new CollectibleItem[3];

        private void Start()
        {
            Debug.Log("=== INVENTARIO TEST ===");
            Debug.Log("Q: Añadir Item 1 | W: Añadir Item 2 | E: Añadir Item 3");
            Debug.Log("R: Remover último | T: Limpiar todo");
            Debug.Log("Y: Guardar | U: Cargar | I: Imprimir estado");
            Debug.Log("======================");

            // Verificar que los items estén asignados
            if (testItems.Length == 0 || testItems[0] == null)
            {
                Debug.LogError("[QuickInventoryTest] ¡IMPORTANTE! Arrastra los 3 CollectibleItems al Inspector.");
                Debug.LogError("Búscalos en: Assets/_Project/Resources/Items/");
            }
        }

        private void Update()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[QuickInventoryTest] InventoryData no encontrado en la escena.");
                return;
            }

            // Añadir items
            if (Input.GetKeyDown(KeyCode.Q))
                AddItem(0);

            if (Input.GetKeyDown(KeyCode.W))
                AddItem(1);

            if (Input.GetKeyDown(KeyCode.E))
                AddItem(2);

            // Remover último
            if (Input.GetKeyDown(KeyCode.R))
                RemoveLastItem();

            // Limpiar todo
            if (Input.GetKeyDown(KeyCode.T))
                ClearAll();

            // Guardar
            if (Input.GetKeyDown(KeyCode.Y))
                SaveInventory();

            // Cargar
            if (Input.GetKeyDown(KeyCode.U))
                LoadInventory();

            // Imprimir
            if (Input.GetKeyDown(KeyCode.I))
                PrintInventory();
        }

        private void AddItem(int index)
        {
            if (index < 0 || index >= testItems.Length)
            {
                Debug.LogWarning($"[TEST] Índice {index} fuera de rango.");
                return;
            }

            if (testItems[index] == null)
            {
                Debug.LogError($"[TEST] No hay item asignado en el índice {index}.");
                Debug.LogError("Arrastra los CollectibleItems al Inspector (Assets/_Project/Resources/Items/)");
                return;
            }

            bool success = InventoryData.Instance.AddItem(testItems[index]);

            if (success)
            {
                Debug.Log($"<color=green>[TEST] ✓ Item añadido: {testItems[index].displayName}</color>");

                // También guardar automáticamente con GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[TEST] ⚠ No se pudo añadir: {testItems[index].displayName} (¿Inventario lleno o duplicado?)</color>");
            }
        }

        private void RemoveLastItem()
        {
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

                // También guardar
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SaveGame();
                }
            }
        }

        private void ClearAll()
        {
            InventoryData.Instance.ClearInventory();
            Debug.Log("<color=cyan>[TEST] 🗑 Inventario limpiado.</color>");

            // También guardar
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
            }
        }

        private void SaveInventory()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();
                Debug.Log("<color=cyan>[TEST] 💾 Partida guardada (con GameManager).</color>");
            }
            else if (InventoryPersistence.Instance != null)
            {
                InventoryPersistence.Instance.SaveInventory();
                Debug.Log("<color=cyan>[TEST] 💾 Inventario guardado.</color>");
            }
            else
            {
                Debug.LogError("[TEST] No se encontró GameManager ni InventoryPersistence.");
            }
        }

        private void LoadInventory()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame();
                Debug.Log("<color=cyan>[TEST] 📂 Partida cargada (con GameManager).</color>");
            }
            else if (InventoryPersistence.Instance != null)
            {
                InventoryPersistence.Instance.LoadInventory();
                Debug.Log("<color=cyan>[TEST] 📂 Inventario cargado.</color>");
            }
            else
            {
                Debug.LogError("[TEST] No se encontró GameManager ni InventoryPersistence.");
            }
        }

        private void PrintInventory()
        {
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
    }
}
