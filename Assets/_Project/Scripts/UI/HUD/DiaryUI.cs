using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Triskel.Core;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la UI del diario narrativo (UI Toolkit).
    ///
    /// RESPONSABILIDADES:
    /// - Mostrar/ocultar panel del diario.
    /// - Listar entradas desbloqueadas.
    /// - Mostrar texto completo de entrada seleccionada con scroll.
    ///
    /// NO DEBE:
    /// - Contener lógica narrativa (eso es DiaryManager).
    /// - Mutar entradas (solo lectura).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class DiaryUI : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Tecla para abrir/cerrar el diario")]
        [SerializeField] private KeyCode toggleKey = KeyCode.J;

        // Referencias UI Toolkit
        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement diaryPanel;
        private VisualElement entryListContainer;
        private Label entryTitleLabel;
        private Label entryTextLabel;
        private ScrollView entryTextScrollView;
        private Button closeButton;

        // Estado
        private bool isPanelOpen = false;
        private DiaryEntryData currentSelectedEntry = null;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            // Esperar un frame para asegurarse de que el UIDocument esté inicializado
            Invoke(nameof(InitializeUI), 0.1f);
        }

        private void InitializeUI()
        {
            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                Debug.LogError("[DiaryUI] UIDocument o rootVisualElement es null.");
                return;
            }

            root = uiDocument.rootVisualElement;

            // Obtener referencias a elementos
            diaryPanel = root.Q<VisualElement>("DiaryPanel");
            entryListContainer = root.Q<VisualElement>("EntryListContainer");
            entryTitleLabel = root.Q<Label>("EntryTitle");
            entryTextLabel = root.Q<Label>("EntryText");
            entryTextScrollView = root.Q<ScrollView>("EntryTextScrollView");
            closeButton = root.Q<Button>("CloseButton");

            // Validar referencias
            if (diaryPanel == null)
            {
                Debug.LogError("[DiaryUI] No se encontró 'DiaryPanel' en el UXML. Verifica el nombre del elemento.");
                return;
            }

            // Registrar eventos
            if (closeButton != null)
            {
                closeButton.clicked += ClosePanel;
            }

            // Inicialmente oculto
            ClosePanel();
        }

        private void Update()
        {
            // Toggle del panel con tecla
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        #region Panel Control

        /// <summary>
        /// Abre/cierra el panel del diario.
        /// </summary>
        public void TogglePanel()
        {
            if (isPanelOpen)
            {
                ClosePanel();
            }
            else
            {
                OpenPanel();
            }
        }

        /// <summary>
        /// Abre el panel del diario y actualiza la lista de entradas.
        /// </summary>
        public void OpenPanel()
        {
            if (diaryPanel == null)
            {
                Debug.LogError("[DiaryUI] DiaryPanel es null. No se puede abrir.");
                return;
            }

            diaryPanel.style.display = DisplayStyle.Flex;
            isPanelOpen = true;

            // Actualizar lista de entradas
            RefreshEntryList();

            Debug.Log("[DiaryUI] Panel del diario abierto.");
        }

        /// <summary>
        /// Cierra el panel del diario.
        /// </summary>
        public void ClosePanel()
        {
            if (diaryPanel == null) return;

            diaryPanel.style.display = DisplayStyle.None;
            isPanelOpen = false;

            Debug.Log("[DiaryUI] Panel del diario cerrado.");
        }

        #endregion

        #region Entry List

        /// <summary>
        /// Actualiza la lista de entradas desbloqueadas.
        /// </summary>
        private void RefreshEntryList()
        {
            if (entryListContainer == null)
            {
                Debug.LogError("[DiaryUI] EntryListContainer es null.");
                return;
            }

            // Limpiar lista actual
            entryListContainer.Clear();

            // Obtener entradas desbloqueadas del DiaryManager
            if (DiaryManager.Instance == null)
            {
                Debug.LogError("[DiaryUI] DiaryManager no está disponible.");
                return;
            }

            List<DiaryEntryData> unlockedEntries = DiaryManager.Instance.GetUnlockedEntries();

            if (unlockedEntries.Count == 0)
            {
                // Mostrar mensaje de "no hay entradas"
                Label emptyLabel = new Label("No hay entradas desbloqueadas aún.");
                emptyLabel.AddToClassList("entry-empty-message");
                entryListContainer.Add(emptyLabel);
                return;
            }

            // Crear botones para cada entrada
            foreach (DiaryEntryData entry in unlockedEntries)
            {
                Button entryButton = CreateEntryButton(entry);
                entryListContainer.Add(entryButton);
            }

            // Seleccionar primera entrada por defecto
            if (unlockedEntries.Count > 0)
            {
                DisplayEntry(unlockedEntries[0]);
            }
        }

        /// <summary>
        /// Crea un botón para una entrada del diario.
        /// </summary>
        private Button CreateEntryButton(DiaryEntryData entry)
        {
            Button button = new Button();
            button.text = $"Nivel {entry.levelIndex + 1}: {entry.title}";
            button.AddToClassList("entry-list-button");

            // Evento al hacer clic
            button.clicked += () => OnEntryButtonClicked(entry);

            return button;
        }

        /// <summary>
        /// Callback cuando se hace clic en un botón de entrada.
        /// </summary>
        private void OnEntryButtonClicked(DiaryEntryData entry)
        {
            DisplayEntry(entry);
        }

        #endregion

        #region Entry Display

        /// <summary>
        /// Muestra el contenido completo de una entrada.
        /// </summary>
        private void DisplayEntry(DiaryEntryData entry)
        {
            if (entry == null)
            {
                Debug.LogWarning("[DiaryUI] Entrada es null, no se puede mostrar.");
                return;
            }

            currentSelectedEntry = entry;

            // Actualizar título
            if (entryTitleLabel != null)
            {
                entryTitleLabel.text = entry.title;
            }

            // Actualizar texto
            if (entryTextLabel != null)
            {
                entryTextLabel.text = entry.text;
            }

            // Scroll al inicio
            if (entryTextScrollView != null)
            {
                entryTextScrollView.scrollOffset = Vector2.zero;
            }

            Debug.Log($"[DiaryUI] Mostrando entrada: '{entry.title}'");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Actualiza la UI del diario (útil cuando se desbloquea una nueva entrada).
        /// </summary>
        public void UpdateUI()
        {
            if (isPanelOpen)
            {
                RefreshEntryList();
            }
        }

        #endregion
    }
}
