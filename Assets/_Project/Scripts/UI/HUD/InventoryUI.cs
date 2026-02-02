using UnityEngine;
using UnityEngine.UIElements;
using Triskel.Core;

namespace Triskel.UI.HUD
{
    /// <summary>
    /// Controlador de la interfaz visual del inventario usando UI Toolkit.
    /// Maneja 3 slots de inventario con soporte para PC y móvil.
    ///
    /// RESPONSABILIDAD: Solo visualización, NO lógica de negocio.
    /// Escucha cambios en InventoryData y actualiza la UI.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private UIDocument uiDocument;

        // Referencias a los elementos UI
        private VisualElement root;
        private Button[] slots = new Button[3];
        private VisualElement[] icons = new VisualElement[3];

        // Estado de selección
        private int currentlySelectedIndex = -1;

        // Guardar referencias a las actions para poder desuscribirlas
        private System.Action[] slotClickActions = new System.Action[3];

        #region Unity Lifecycle

        private void OnEnable()
        {
            if (uiDocument == null) return;

            root = uiDocument.rootVisualElement;

            // Obtener referencias a slots e iconos
            for (int i = 0; i < 3; i++)
            {
                slots[i] = root.Q<Button>($"Slot{i + 1}");
                icons[i] = root.Q<VisualElement>($"Icon{i + 1}");

                // Suscribirse a eventos de clic (capturar índice para lambda)
                int index = i;
                if (slots[i] != null)
                {
                    slotClickActions[index] = () => OnSlotClicked(slots[index], index);
                    slots[i].clicked += slotClickActions[index];
                }
            }
        }

        private void Start()
        {
            // Suscribirse a eventos de InventoryData (después de Awake)
            SubscribeToInventoryEvents();

            // Cargar estado actual del inventario
            RefreshUI();
        }

        private void OnDisable()
        {
            // Desuscribirse de eventos para evitar memory leaks
            for (int i = 0; i < 3; i++)
            {
                if (slots[i] != null && slotClickActions[i] != null)
                {
                    slots[i].clicked -= slotClickActions[i];
                }
            }

            // Desuscribirse de eventos de InventoryData
            UnsubscribeFromInventoryEvents();
        }

        // NOTA: Selección de slots con teclado 1/2/3 ahora se maneja desde RelicSystem
        // con el nuevo Input System, por lo que no necesitamos Update() aquí.

        #endregion

        #region Inventory Events

        /// <summary>
        /// Suscribirse a eventos de InventoryData.
        /// </summary>
        private void SubscribeToInventoryEvents()
        {
            if (InventoryData.Instance != null)
            {
                InventoryData.Instance.OnItemAdded += HandleItemAdded;
                InventoryData.Instance.OnItemRemoved += HandleItemRemoved;
                InventoryData.Instance.OnInventoryCleared += HandleInventoryCleared;
            }
        }

        /// <summary>
        /// Desuscribirse de eventos de InventoryData.
        /// </summary>
        private void UnsubscribeFromInventoryEvents()
        {
            if (InventoryData.Instance != null)
            {
                InventoryData.Instance.OnItemAdded -= HandleItemAdded;
                InventoryData.Instance.OnItemRemoved -= HandleItemRemoved;
                InventoryData.Instance.OnInventoryCleared -= HandleInventoryCleared;
            }
        }

        /// <summary>
        /// Manejador cuando se añade un item al inventario.
        /// </summary>
        private void HandleItemAdded(CollectibleItem item)
        {
            Debug.Log($"[InventoryUI] Item añadido: {item.displayName}");
            RefreshUI();
        }

        /// <summary>
        /// Manejador cuando se remueve un item del inventario.
        /// </summary>
        private void HandleItemRemoved(CollectibleItem item)
        {
            Debug.Log($"[InventoryUI] Item removido: {item.displayName}");
            RefreshUI();
        }

        /// <summary>
        /// Manejador cuando se limpia el inventario.
        /// </summary>
        private void HandleInventoryCleared()
        {
            Debug.Log("[InventoryUI] Inventario limpiado");
            RefreshUI();
        }

        #endregion

        #region UI Update Methods

        /// <summary>
        /// Refresca toda la UI según el estado actual de InventoryData.
        /// </summary>
        public void RefreshUI()
        {
            if (InventoryData.Instance == null)
            {
                Debug.LogWarning("[InventoryUI] InventoryData no está inicializado.");
                return;
            }

            // Limpiar todos los slots primero
            for (int i = 0; i < 3; i++)
            {
                ClearSlot(i);
            }

            // Cargar items del inventario
            var items = InventoryData.Instance.GetAllItems();
            for (int i = 0; i < items.Count && i < 3; i++)
            {
                CollectibleItem item = items[i];
                SetSlot(i, item);
            }
        }

        /// <summary>
        /// Configura un slot con un item.
        /// </summary>
        private void SetSlot(int slotIndex, CollectibleItem item)
        {
            // Asignar icono
            SetItemIcon(slotIndex, item.icon);

            // Asignar color del slot
            SetSlotColor(slotIndex, item.slotColor);
        }

        /// <summary>
        /// Limpia un slot (vacío).
        /// </summary>
        private void ClearSlot(int slotIndex)
        {
            ClearItemIcon(slotIndex);
            SetSlotColor(slotIndex, "grey");
        }

        #endregion

        #region Slot Interaction

        /// <summary>
        /// Se ejecuta cuando se hace clic en un slot.
        /// Funciona tanto en PC (clic) como en móvil (touch).
        /// </summary>
        private void OnSlotClicked(Button clickedSlot, int slotIndex)
        {
            // Verificar que el slot tenga un item
            if (InventoryData.Instance == null) return;

            CollectibleItem item = InventoryData.Instance.GetItemAtSlot(slotIndex);
            if (item == null)
            {
                Debug.Log($"[InventoryUI] Slot {slotIndex + 1} está vacío.");
                return;
            }

            Debug.Log($"[InventoryUI] Slot {slotIndex + 1} seleccionado: {item.displayName}");

            // Toggle de selección
            if (currentlySelectedIndex == slotIndex)
            {
                // Deseleccionar
                DeselectSlot(clickedSlot);
                currentlySelectedIndex = -1;

                // Aquí puedes añadir lógica cuando se deselecciona (ejemplo: guardar arma)
                OnItemDeselected(item);
            }
            else
            {
                // Deseleccionar el slot anterior si existe
                if (currentlySelectedIndex >= 0 && currentlySelectedIndex < slots.Length)
                {
                    DeselectSlot(slots[currentlySelectedIndex]);
                }

                // Seleccionar el nuevo slot
                SelectSlot(clickedSlot);
                currentlySelectedIndex = slotIndex;

                // Aquí puedes añadir lógica cuando se selecciona (ejemplo: equipar arma)
                OnItemSelected(item, slotIndex);
            }
        }

        /// <summary>
        /// Selecciona un slot por su índice (usado para teclado).
        /// Solo puede haber un slot seleccionado a la vez.
        /// </summary>
        public void SelectSlotByIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex] == null) return;

            OnSlotClicked(slots[slotIndex], slotIndex);
        }

        /// <summary>
        /// Actualiza solo el visual del slot seleccionado sin disparar lógica de gameplay.
        /// Usado cuando la selección viene desde RelicSystem para evitar bucles.
        /// </summary>
        public void UpdateSlotVisual(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex] == null) return;

            // Verificar que el slot tenga un item
            if (InventoryData.Instance == null) return;
            CollectibleItem item = InventoryData.Instance.GetItemAtSlot(slotIndex);
            if (item == null) return;

            // Deseleccionar el slot anterior si existe
            if (currentlySelectedIndex >= 0 && currentlySelectedIndex < slots.Length)
            {
                DeselectSlot(slots[currentlySelectedIndex]);
            }

            // Seleccionar el nuevo slot visualmente
            SelectSlot(slots[slotIndex]);
            currentlySelectedIndex = slotIndex;
        }

        /// <summary>
        /// Deselecciona todos los slots visualmente sin disparar lógica de gameplay.
        /// Usado cuando se deselecciona desde RelicSystem con teclas.
        /// </summary>
        public void DeselectAllSlots()
        {
            // Deseleccionar el slot actual si existe
            if (currentlySelectedIndex >= 0 && currentlySelectedIndex < slots.Length)
            {
                DeselectSlot(slots[currentlySelectedIndex]);
            }

            currentlySelectedIndex = -1;
        }

        #endregion

        #region Visual Methods

        /// <summary>
        /// Marca un slot como seleccionado visualmente.
        /// </summary>
        private void SelectSlot(Button slot)
        {
            slot.AddToClassList("selected");
        }

        /// <summary>
        /// Quita la selección visual de un slot.
        /// </summary>
        private void DeselectSlot(Button slot)
        {
            slot.RemoveFromClassList("selected");
        }

        /// <summary>
        /// Asigna un sprite a un slot específico.
        /// </summary>
        private void SetItemIcon(int slotIndex, Sprite sprite)
        {
            if (slotIndex < 0 || slotIndex >= icons.Length) return;
            if (icons[slotIndex] == null || sprite == null) return;

            icons[slotIndex].style.backgroundImage = new StyleBackground(sprite);
        }

        /// <summary>
        /// Cambia el color de un slot (blue, green, red, yellow, grey).
        /// </summary>
        private void SetSlotColor(int slotIndex, string color)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return;
            if (slots[slotIndex] == null) return;

            // Remover todas las clases de color anteriores
            slots[slotIndex].RemoveFromClassList("blue");
            slots[slotIndex].RemoveFromClassList("green");
            slots[slotIndex].RemoveFromClassList("red");
            slots[slotIndex].RemoveFromClassList("yellow");

            // Agregar la nueva clase de color (si no es grey)
            if (color != "grey")
            {
                slots[slotIndex].AddToClassList(color);
            }
        }

        /// <summary>
        /// Limpia el icono de un slot.
        /// </summary>
        private void ClearItemIcon(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= icons.Length) return;
            if (icons[slotIndex] == null) return;

            icons[slotIndex].style.backgroundImage = null;
        }

        #endregion

        #region Game Logic Hooks

        /// <summary>
        /// Se ejecuta cuando un item es seleccionado.
        /// AQUÍ puedes añadir lógica de gameplay (equipar arma, activar habilidad, etc.)
        /// </summary>
        private void OnItemSelected(CollectibleItem item, int slotIndex)
        {
            Debug.Log($"[InventoryUI] Item seleccionado: {item.displayName} en slot {slotIndex}");

            // Integración con RelicSystem - Seleccionar reliquia según slot
            RelicSystem relicSystem = FindFirstObjectByType<RelicSystem>();
            if (relicSystem != null)
            {
                // Mapear slot index a tipo de reliquia
                // Asumiendo: Slot 0 = Lirio, Slot 1 = Hacha, Slot 2 = Manto
                RelicSystem.RelicType relicType = (RelicSystem.RelicType)(slotIndex + 1);
                relicSystem.SelectRelic(relicType);
                Debug.Log($"[InventoryUI] Reliquia seleccionada: {relicType}");
            }
        }

        /// <summary>
        /// Se ejecuta cuando un item es deseleccionado.
        /// </summary>
        private void OnItemDeselected(CollectibleItem item)
        {
            Debug.Log($"[InventoryUI] Item deseleccionado: {item.displayName}");

            // Sincronizar con RelicSystem - Deseleccionar reliquia
            RelicSystem relicSystem = FindFirstObjectByType<RelicSystem>();
            if (relicSystem != null)
            {
                relicSystem.SelectRelic(RelicSystem.RelicType.None);
                Debug.Log("[InventoryUI] Reliquia deseleccionada");
            }
        }

        #endregion
    }
}
