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
        private Button slot1;
        private Button slot2;
        private Button slot3;
        private VisualElement icon1;
        private VisualElement icon2;
        private VisualElement icon3;

        // Estado de selección
        private Button currentlySelectedSlot;
        private int currentlySelectedIndex = -1;

        #region Unity Lifecycle

        private void OnEnable()
        {
            // Obtener la raíz del documento UI
            root = uiDocument.rootVisualElement;

            // Obtener referencias a los slots (botones)
            slot1 = root.Q<Button>("Slot1");
            slot2 = root.Q<Button>("Slot2");
            slot3 = root.Q<Button>("Slot3");

            // Obtener referencias a los iconos
            icon1 = root.Q<VisualElement>("Icon1");
            icon2 = root.Q<VisualElement>("Icon2");
            icon3 = root.Q<VisualElement>("Icon3");

            // Suscribirse a eventos de clic
            slot1.clicked += () => OnSlotClicked(slot1, 0);
            slot2.clicked += () => OnSlotClicked(slot2, 1);
            slot3.clicked += () => OnSlotClicked(slot3, 2);

            // Suscribirse a eventos de InventoryData
            SubscribeToInventoryEvents();

            // Cargar estado actual del inventario
            RefreshUI();
        }

        private void OnDisable()
        {
            // Desuscribirse de eventos para evitar memory leaks
            if (slot1 != null) slot1.clicked -= () => OnSlotClicked(slot1, 0);
            if (slot2 != null) slot2.clicked -= () => OnSlotClicked(slot2, 1);
            if (slot3 != null) slot3.clicked -= () => OnSlotClicked(slot3, 2);

            // Desuscribirse de eventos de InventoryData
            UnsubscribeFromInventoryEvents();
        }

        private void Update()
        {
            // Control con teclado: Teclas 1, 2, 3
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                SelectSlotByIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                SelectSlotByIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                SelectSlotByIndex(2);
            }
        }

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
                currentlySelectedSlot = null;
                currentlySelectedIndex = -1;

                // Aquí puedes añadir lógica cuando se deselecciona (ejemplo: guardar arma)
                OnItemDeselected(item);
            }
            else
            {
                // Deseleccionar el slot anterior
                if (currentlySelectedSlot != null)
                {
                    DeselectSlot(currentlySelectedSlot);
                }

                // Seleccionar el nuevo slot
                SelectSlot(clickedSlot);
                currentlySelectedSlot = clickedSlot;
                currentlySelectedIndex = slotIndex;

                // Aquí puedes añadir lógica cuando se selecciona (ejemplo: equipar arma)
                OnItemSelected(item, slotIndex);
            }
        }

        /// <summary>
        /// Selecciona un slot por su índice (usado para teclado).
        /// Solo puede haber un slot seleccionado a la vez.
        /// </summary>
        private void SelectSlotByIndex(int slotIndex)
        {
            Button slotToSelect = slotIndex switch
            {
                0 => slot1,
                1 => slot2,
                2 => slot3,
                _ => null
            };

            if (slotToSelect == null) return;

            // Simular clic
            OnSlotClicked(slotToSelect, slotIndex);
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
            VisualElement icon = slotIndex switch
            {
                0 => icon1,
                1 => icon2,
                2 => icon3,
                _ => null
            };

            if (icon != null && sprite != null)
            {
                // Convertir Sprite a Background para UI Toolkit
                icon.style.backgroundImage = new StyleBackground(sprite);
            }
        }

        /// <summary>
        /// Cambia el color de un slot (blue, green, red, yellow, grey).
        /// </summary>
        private void SetSlotColor(int slotIndex, string color)
        {
            Button slot = slotIndex switch
            {
                0 => slot1,
                1 => slot2,
                2 => slot3,
                _ => null
            };

            if (slot == null) return;

            // Remover todas las clases de color anteriores
            slot.RemoveFromClassList("blue");
            slot.RemoveFromClassList("green");
            slot.RemoveFromClassList("red");
            slot.RemoveFromClassList("yellow");

            // Agregar la nueva clase de color (si no es grey)
            if (color != "grey")
            {
                slot.AddToClassList(color);
            }
        }

        /// <summary>
        /// Limpia el icono de un slot.
        /// </summary>
        private void ClearItemIcon(int slotIndex)
        {
            VisualElement icon = slotIndex switch
            {
                0 => icon1,
                1 => icon2,
                2 => icon3,
                _ => null
            };

            if (icon != null)
            {
                icon.style.backgroundImage = null;
            }
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

            // TODO: Añadir lógica de gameplay
            // Ejemplo: PlayerController.Instance.EquipItem(item);
            // Ejemplo: RelicSystem.Instance.ActivateRelic(item.itemID);
        }

        /// <summary>
        /// Se ejecuta cuando un item es deseleccionado.
        /// </summary>
        private void OnItemDeselected(CollectibleItem item)
        {
            Debug.Log($"[InventoryUI] Item deseleccionado: {item.displayName}");

            // TODO: Añadir lógica de gameplay
            // Ejemplo: PlayerController.Instance.UnequipItem();
        }

        /// <summary>
        /// Obtiene el índice del slot actualmente seleccionado.
        /// </summary>
        public int GetSelectedSlotIndex()
        {
            return currentlySelectedIndex;
        }

        /// <summary>
        /// Obtiene el item actualmente seleccionado.
        /// </summary>
        public CollectibleItem GetSelectedItem()
        {
            if (currentlySelectedIndex >= 0 && InventoryData.Instance != null)
            {
                return InventoryData.Instance.GetItemAtSlot(currentlySelectedIndex);
            }
            return null;
        }

        #endregion
    }
}
