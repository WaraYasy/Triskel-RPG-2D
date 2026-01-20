using UnityEngine;
using UnityEngine.UIElements;
using Triskel.Core;
using Triskel.UI.HUD;

namespace Triskel.Testing
{
    /// <summary>
    /// Diagnóstico específico para InventoryUI.
    /// Identifica por qué los items no se ven en pantalla.
    /// </summary>
    public class InventoryUIDebugger : MonoBehaviour
    {
        private void Start()
        {
            Invoke(nameof(RunDiagnostic), 1f); // Esperar a que todo se inicialice
        }

        [ContextMenu("🔍 Diagnosticar InventoryUI")]
        public void RunDiagnostic()
        {
            Debug.Log("<color=yellow>═══════════════════════════════════════════</color>");
            Debug.Log("<color=yellow>  🔍 DIAGNÓSTICO DE INVENTORYUI</color>");
            Debug.Log("<color=yellow>═══════════════════════════════════════════</color>\n");

            CheckInventoryData();
            CheckInventoryUI();
            CheckUIDocument();
            CheckUIElements();
            CheckEvents();

            Debug.Log("<color=yellow>═══════════════════════════════════════════</color>");
        }

        private void CheckInventoryData()
        {
            Debug.Log("<color=cyan>━━━ 1. INVENTORYDATA ━━━</color>");

            if (InventoryData.Instance == null)
            {
                Debug.LogError("  ✗ InventoryData NO encontrado");
                return;
            }

            Debug.Log($"  ✓ InventoryData encontrado en: {InventoryData.Instance.gameObject.name}");

            var items = InventoryData.Instance.GetAllItems();
            Debug.Log($"  → Items en inventario: {items.Count}/3");

            if (items.Count == 0)
            {
                Debug.LogWarning("  ⚠ El inventario está vacío. Presiona Q/W/E para añadir items.");
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    bool hasIcon = item.icon != null;
                    bool hasColor = !string.IsNullOrEmpty(item.slotColor);

                    Debug.Log($"  Slot {i}: {item.displayName}");
                    Debug.Log($"    - Icon: {(hasIcon ? "✓" : "✗ FALTA")}");
                    Debug.Log($"    - SlotColor: {(hasColor ? item.slotColor : "✗ FALTA")}");

                    if (!hasIcon)
                    {
                        Debug.LogError($"    → ERROR: {item.displayName} no tiene sprite asignado.");
                        Debug.LogError($"       Abre el asset en Inspector y asigna un sprite en el campo 'Icon'");
                    }
                }
            }

            Debug.Log("");
        }

        private void CheckInventoryUI()
        {
            Debug.Log("<color=cyan>━━━ 2. INVENTORYUI SCRIPT ━━━</color>");

            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();

            if (inventoryUI == null)
            {
                Debug.LogError("  ✗ InventoryUI script NO encontrado");
                Debug.LogError("  → SOLUCIÓN: Crea un GameObject y añádele el componente InventoryUI");
                return;
            }

            Debug.Log($"  ✓ InventoryUI encontrado en: {inventoryUI.gameObject.name}");

            // Verificar si está activo
            if (!inventoryUI.enabled)
            {
                Debug.LogError("  ✗ InventoryUI está DESACTIVADO");
                Debug.LogError("  → SOLUCIÓN: Activa el componente en el Inspector");
            }
            else
            {
                Debug.Log("  ✓ InventoryUI está activo");
            }

            Debug.Log("");
        }

        private void CheckUIDocument()
        {
            Debug.Log("<color=cyan>━━━ 3. UIDOCUMENT ━━━</color>");

            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null) return;

            UIDocument uiDoc = inventoryUI.GetComponent<UIDocument>();

            if (uiDoc == null)
            {
                Debug.LogError("  ✗ UIDocument NO encontrado en el mismo GameObject que InventoryUI");
                Debug.LogError("  → SOLUCIÓN: Añade un componente UIDocument al GameObject");
                return;
            }

            Debug.Log("  ✓ UIDocument encontrado");

            // Verificar si está asignado en el campo serializado de InventoryUI
            var field = typeof(InventoryUI).GetField("uiDocument", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                UIDocument assignedDoc = field.GetValue(inventoryUI) as UIDocument;
                if (assignedDoc == null)
                {
                    Debug.LogError("  ✗ El campo 'uiDocument' en InventoryUI es NULL");
                    Debug.LogError("  → SOLUCIÓN: En Inspector, arrastra el componente UIDocument al campo 'Ui Document'");
                    return;
                }
                else
                {
                    Debug.Log("  ✓ Campo 'uiDocument' asignado correctamente");
                }
            }

            // Verificar Visual Tree Asset
            if (uiDoc.visualTreeAsset == null)
            {
                Debug.LogError("  ✗ Visual Tree Asset (UXML) NO asignado");
                Debug.LogError("  → SOLUCIÓN: En UIDocument, arrastra 'Inventory.uxml' al campo 'Source Asset'");
                Debug.LogError("     Ubicación: Assets/_Project/UI/HUD/Inventory.uxml");
                return;
            }

            Debug.Log($"  ✓ Visual Tree Asset asignado: {uiDoc.visualTreeAsset.name}");

            // Verificar Panel Settings
            if (uiDoc.panelSettings == null)
            {
                Debug.LogWarning("  ⚠ Panel Settings NO asignado (puede causar problemas)");
                Debug.LogWarning("  → Asigna un PanelSettings al UIDocument");
            }
            else
            {
                Debug.Log("  ✓ Panel Settings asignado");
            }

            Debug.Log("");
        }

        private void CheckUIElements()
        {
            Debug.Log("<color=cyan>━━━ 4. ELEMENTOS UI (UXML) ━━━</color>");

            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null) return;

            UIDocument uiDoc = inventoryUI.GetComponent<UIDocument>();
            if (uiDoc == null || uiDoc.rootVisualElement == null) return;

            VisualElement root = uiDoc.rootVisualElement;

            // Buscar los elementos que InventoryUI necesita
            Debug.Log("  Buscando elementos en el UXML:");

            for (int i = 1; i <= 3; i++)
            {
                var slot = root.Q<Button>($"Slot{i}");
                var icon = root.Q<VisualElement>($"Icon{i}");

                Debug.Log($"  Slot{i}: {(slot != null ? "✓" : "✗ NO ENCONTRADO")}");
                Debug.Log($"  Icon{i}: {(icon != null ? "✓" : "✗ NO ENCONTRADO")}");

                if (slot == null)
                {
                    Debug.LogError($"    → ERROR: No se encontró el botón 'Slot{i}' en el UXML");
                    Debug.LogError($"       Verifica que Inventory.uxml tenga un <Button name='Slot{i}'>");
                }

                if (icon == null)
                {
                    Debug.LogError($"    → ERROR: No se encontró el elemento 'Icon{i}' en el UXML");
                    Debug.LogError($"       Verifica que Inventory.uxml tenga un <VisualElement name='Icon{i}'>");
                }

                // Si el icono existe, verificar si tiene background-image
                if (icon != null)
                {
                    var bgImage = icon.style.backgroundImage;
                    if (bgImage.value.sprite != null)
                    {
                        Debug.Log($"    Icon{i} tiene sprite: {bgImage.value.sprite.name}");
                    }
                    else
                    {
                        Debug.Log($"    Icon{i} sin sprite (normal si el slot está vacío)");
                    }
                }
            }

            Debug.Log("");
        }

        private void CheckEvents()
        {
            Debug.Log("<color=cyan>━━━ 5. EVENTOS ━━━</color>");

            if (InventoryData.Instance == null)
            {
                Debug.LogWarning("  ⚠ No se puede verificar eventos sin InventoryData");
                return;
            }

            // Verificar que InventoryUI esté suscrito a los eventos
            var onItemAddedField = typeof(InventoryData).GetField("OnItemAdded");
            if (onItemAddedField != null)
            {
                var subscribers = onItemAddedField.GetValue(InventoryData.Instance) as System.Delegate;
                int subscriberCount = subscribers?.GetInvocationList().Length ?? 0;

                if (subscriberCount > 0)
                {
                    Debug.Log($"  ✓ OnItemAdded tiene {subscriberCount} suscriptor(es)");
                }
                else
                {
                    Debug.LogError("  ✗ OnItemAdded NO tiene suscriptores");
                    Debug.LogError("  → InventoryUI no está escuchando los cambios del inventario");
                }
            }

            Debug.Log("");
        }

        [ContextMenu("⚡ Test: Forzar RefreshUI")]
        public void ForceRefreshUI()
        {
            InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
            if (inventoryUI == null)
            {
                Debug.LogError("[Test] InventoryUI no encontrado");
                return;
            }

            Debug.Log("<color=cyan>[Test] Forzando RefreshUI()...</color>");
            inventoryUI.RefreshUI();
            Debug.Log("<color=cyan>[Test] RefreshUI() ejecutado</color>");
        }
    }
}
