using UnityEngine;
using UnityEngine.UIElements;
using Triskel.UI.HUD;

namespace Triskel.Core
{
    /// <summary>
    /// Script de diagnóstico para el sistema de inventario.
    /// Detecta problemas comunes y los reporta en la consola.
    /// </summary>
    public class InventoryDebugger : MonoBehaviour
    {
        private void Start()
        {
            // Esperar un frame para que todo se inicialice
            Invoke(nameof(RunDiagnostics), 0.5f);
        }

        [ContextMenu("🔍 Diagnosticar Sistema de Inventario")]
        public void RunDiagnostics()
        {
            Debug.Log("<color=yellow>═══════════════════════════════════════════</color>");
            Debug.Log("<color=yellow>  🔍 DIAGNÓSTICO DEL SISTEMA DE INVENTARIO</color>");
            Debug.Log("<color=yellow>═══════════════════════════════════════════</color>\n");

            CheckInventoryData();
            CheckInventoryUI();
            CheckUIDocument();
            CheckCollectibleItems();

            Debug.Log("<color=yellow>═══════════════════════════════════════════</color>");
            Debug.Log("<color=cyan>Diagnóstico completado. Revisa los mensajes arriba.</color>");
        }

        private void CheckInventoryData()
        {
            Debug.Log("<color=cyan>━━━ 1. INVENTORYDATA ━━━</color>");

            if (InventoryData.Instance == null)
            {
                Debug.LogError("  ✗ InventoryData NO encontrado");
                Debug.LogError("  → SOLUCIÓN: Crea un GameObject y añádele el componente InventoryData");
                return;
            }

            Debug.Log($"  ✓ InventoryData encontrado en: {InventoryData.Instance.gameObject.name}");

            // Verificar items en el inventario
            var items = InventoryData.Instance.GetAllItems();
            int itemCount = items.Count;
            Debug.Log($"  → Items en inventario: {itemCount}/3");

            if (itemCount > 0)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    Debug.Log($"    Slot {i}: {item.displayName} (icon: {(item.icon != null ? "✓" : "✗")})");
                }
            }
            else
            {
                Debug.LogWarning("  ⚠ El inventario está vacío. Usa InventoryTester para añadir items.");
            }

            Debug.Log("");
        }

        private void CheckInventoryUI()
        {
            Debug.Log("<color=cyan>━━━ 2. INVENTORYUI ━━━</color>");

            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();

            if (inventoryUI == null)
            {
                Debug.LogError("  ✗ InventoryUI NO encontrado");
                Debug.LogError("  → SOLUCIÓN: Añade el componente InventoryUI al GameObject HUD");
                return;
            }

            Debug.Log($"  ✓ InventoryUI encontrado en: {inventoryUI.gameObject.name}");

            // Usar reflection para verificar la referencia privada
            var field = typeof(InventoryUI).GetField("uiDocument", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                UIDocument uiDoc = field.GetValue(inventoryUI) as UIDocument;
                if (uiDoc == null)
                {
                    Debug.LogError("  ✗ El campo 'uiDocument' es NULL");
                    Debug.LogError("  → SOLUCIÓN: En el Inspector de InventoryUI, arrastra el componente UIDocument al campo 'Ui Document'");
                }
                else
                {
                    Debug.Log("  ✓ Campo uiDocument asignado correctamente");
                }
            }

            Debug.Log("");
        }

        private void CheckUIDocument()
        {
            Debug.Log("<color=cyan>━━━ 3. UIDOCUMENT ━━━</color>");

            GameObject hud = GameObject.Find("HUD");
            if (hud == null)
            {
                Debug.LogError("  ✗ GameObject 'HUD' NO encontrado");
                Debug.LogError("  → SOLUCIÓN: Renombra tu GameObject de UI a 'HUD' o actualiza el código");
                return;
            }

            Debug.Log($"  ✓ GameObject HUD encontrado");

            UIDocument uiDoc = hud.GetComponent<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogError("  ✗ UIDocument NO encontrado en HUD");
                Debug.LogError("  → SOLUCIÓN: Añade un componente UIDocument al GameObject HUD");
                return;
            }

            Debug.Log("  ✓ UIDocument encontrado");

            // Verificar visualTreeAsset (sourceAsset en versiones antiguas)
            var visualTreeAsset = uiDoc.visualTreeAsset;
            if (visualTreeAsset == null)
            {
                Debug.LogError("  ✗ Visual Tree Asset es NULL");
                Debug.LogError("  → SOLUCIÓN: En UIDocument, asigna 'Inventory.uxml' al campo Source Asset");
                return;
            }

            Debug.Log($"  ✓ Visual Tree Asset asignado: {visualTreeAsset.name}");

            // Verificar PanelSettings
            if (uiDoc.panelSettings == null)
            {
                Debug.LogWarning("  ⚠ Panel Settings es NULL (puede causar problemas)");
                Debug.LogWarning("  → SOLUCIÓN: Asigna un PanelSettings al UIDocument");
            }
            else
            {
                Debug.Log($"  ✓ Panel Settings asignado");
            }

            // Verificar estructura del UI
            VisualElement root = uiDoc.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("  ✗ rootVisualElement es NULL");
                return;
            }

            Debug.Log("  ✓ rootVisualElement existe");

            // Buscar elementos específicos
            var container = root.Q<VisualElement>("InventoryContainer");
            var slot1 = root.Q<Button>("Slot1");
            var slot2 = root.Q<Button>("Slot2");
            var slot3 = root.Q<Button>("Slot3");
            var icon1 = root.Q<VisualElement>("Icon1");
            var icon2 = root.Q<VisualElement>("Icon2");
            var icon3 = root.Q<VisualElement>("Icon3");

            Debug.Log($"    InventoryContainer: {(container != null ? "✓" : "✗")}");
            Debug.Log($"    Slot1 Button: {(slot1 != null ? "✓" : "✗")}");
            Debug.Log($"    Slot2 Button: {(slot2 != null ? "✓" : "✗")}");
            Debug.Log($"    Slot3 Button: {(slot3 != null ? "✓" : "✗")}");
            Debug.Log($"    Icon1: {(icon1 != null ? "✓" : "✗")}");
            Debug.Log($"    Icon2: {(icon2 != null ? "✓" : "✗")}");
            Debug.Log($"    Icon3: {(icon3 != null ? "✓" : "✗")}");

            if (container == null || slot1 == null)
            {
                Debug.LogError("  ✗ NO se pudieron encontrar los elementos UI");
                Debug.LogError("  → PROBLEMA: El archivo UXML podría estar mal configurado");
                Debug.LogError("  → Verifica que Inventory.uxml tenga los nombres correctos (Slot1, Icon1, etc.)");
            }

            // Verificar si los iconos tienen background-image asignado
            if (icon1 != null)
            {
                var bgImage = icon1.style.backgroundImage;
                Debug.Log($"    Icon1 background-image: {(bgImage.value.sprite != null ? "✓ Asignado" : "✗ Vacío")}");
            }

            Debug.Log("");
        }

        private void CheckCollectibleItems()
        {
            Debug.Log("<color=cyan>━━━ 4. COLLECTIBLE ITEMS ━━━</color>");

            if (InventoryPersistence.Instance == null)
            {
                Debug.LogWarning("  ⚠ InventoryPersistence NO encontrado");
                Debug.LogWarning("  → Esto es opcional, pero necesario para guardar/cargar");
                Debug.Log("");
                return;
            }

            Debug.Log($"  ✓ InventoryPersistence encontrado en: {InventoryPersistence.Instance.gameObject.name}");

            // Usar reflection para verificar availableItems
            var field = typeof(InventoryPersistence).GetField("availableItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                CollectibleItem[] items = field.GetValue(InventoryPersistence.Instance) as CollectibleItem[];
                if (items == null || items.Length == 0)
                {
                    Debug.LogWarning("  ⚠ availableItems está vacío");
                    Debug.LogWarning("  → Asigna los CollectibleItems en el Inspector de InventoryPersistence");
                }
                else
                {
                    Debug.Log($"  ✓ Available Items: {items.Length}");
                    foreach (var item in items)
                    {
                        if (item != null)
                        {
                            Debug.Log($"    - {item.displayName} (icon: {(item.icon != null ? "✓" : "✗")})");
                        }
                    }
                }
            }

            Debug.Log("");
        }
    }
}
