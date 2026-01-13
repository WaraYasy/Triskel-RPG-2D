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
        private Label entryTitleLabel;
        private Label entryTextLabel;
        private ScrollView entryTextScrollView;
        private Label pageIndicatorLabel;
        private VisualElement closeButton;
        private Button prevButton;
        private Button nextButton;

        // Estado
        private bool isPanelOpen = false;
        private List<DiaryEntry> unlockedEntries = new List<DiaryEntry>();
        private int currentPageIndex = 0;
        private bool isInitialized = false;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            // Esperar un frame para asegurarse de que el UIDocument esté inicializado
            if (!isInitialized)
            {
                Invoke(nameof(InitializeUI), 0.1f);
            }
        }

        private void OnDisable()
        {
            // Limpiar eventos al desactivar
            UnregisterEvents();
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
            entryTitleLabel = root.Q<Label>("EntryTitle");
            entryTextLabel = root.Q<Label>("EntryText");
            entryTextScrollView = root.Q<ScrollView>("EntryTextScrollView");
            pageIndicatorLabel = root.Q<Label>("PageIndicator");
            closeButton = root.Q<VisualElement>("CloseButton");
            prevButton = root.Q<Button>("PrevButton");
            nextButton = root.Q<Button>("NextButton");

            // Validar referencias
            if (diaryPanel == null)
            {
                Debug.LogError("[DiaryUI] No se encontró 'DiaryPanel' en el UXML. Verifica el nombre del elemento.");
                return;
            }

            // Registrar eventos
            RegisterEvents();

            // Inicialmente oculto
            ClosePanel();

            isInitialized = true;
            Debug.Log("[DiaryUI] UI inicializada correctamente.");
        }

        private void RegisterEvents()
        {
            if (closeButton != null)
            {
                closeButton.RegisterCallback<ClickEvent>(OnCloseButtonClicked);
                Debug.Log($"[DiaryUI] Botón cerrar registrado. Visible: {closeButton.visible}");
            }
            else
            {
                Debug.LogError("[DiaryUI] No se encontró el botón 'CloseButton' en el UXML.");
            }

            if (prevButton != null)
            {
                prevButton.clicked += ShowPreviousPage;
            }

            if (nextButton != null)
            {
                nextButton.clicked += ShowNextPage;
            }
        }

        private void UnregisterEvents()
        {
            if (closeButton != null)
            {
                closeButton.UnregisterCallback<ClickEvent>(OnCloseButtonClicked);
            }

            if (prevButton != null)
            {
                prevButton.clicked -= ShowPreviousPage;
            }

            if (nextButton != null)
            {
                nextButton.clicked -= ShowNextPage;
            }
        }

        private void OnCloseButtonClicked(ClickEvent evt)
        {
            Debug.Log("[DiaryUI] ¡Botón X clickeado!");
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
        /// Abre el panel del diario y muestra las entradas.
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

            // Cargar entradas desbloqueadas
            LoadUnlockedEntries();

            // Mostrar última página (entrada más reciente)
            currentPageIndex = Mathf.Max(0, unlockedEntries.Count - 1);
            ShowCurrentPage();

            Debug.Log("[DiaryUI] Panel del diario abierto.");
        }

        /// <summary>
        /// Cierra el panel del diario.
        /// </summary>
        public void ClosePanel()
        {
            Debug.Log("[DiaryUI] ClosePanel() llamado.");

            if (diaryPanel == null)
            {
                Debug.LogError("[DiaryUI] diaryPanel es null, no se puede cerrar.");
                return;
            }

            diaryPanel.style.display = DisplayStyle.None;
            isPanelOpen = false;

            Debug.Log("[DiaryUI] ✓ Panel del diario cerrado exitosamente.");
        }

        #endregion

        #region Entry Loading and Navigation

        /// <summary>
        /// Carga las entradas desbloqueadas del DiaryManager.
        /// </summary>
        private void LoadUnlockedEntries()
        {
            if (DiaryManager.Instance == null)
            {
                Debug.LogError("[DiaryUI] DiaryManager no está disponible.");
                unlockedEntries.Clear();
                return;
            }

            unlockedEntries = DiaryManager.Instance.GetUnlockedEntries();
            Debug.Log($"[DiaryUI] Entradas cargadas: {unlockedEntries.Count}");
        }

        /// <summary>
        /// Muestra la página actual (entrada).
        /// </summary>
        private void ShowCurrentPage()
        {
            if (unlockedEntries.Count == 0)
            {
                // No hay entradas desbloqueadas
                ShowEmptyState();
                UpdateNavigationButtons();
                return;
            }

            // Asegurar que el índice está en rango
            currentPageIndex = Mathf.Clamp(currentPageIndex, 0, unlockedEntries.Count - 1);

            // Mostrar entrada actual
            DiaryEntry currentEntry = unlockedEntries[currentPageIndex];
            DisplayEntry(currentEntry);

            // Actualizar indicador de página
            UpdatePageIndicator();

            // Actualizar estado de botones de navegación
            UpdateNavigationButtons();
        }

        /// <summary>
        /// Muestra mensaje cuando no hay entradas.
        /// </summary>
        private void ShowEmptyState()
        {
            if (entryTitleLabel != null)
            {
                entryTitleLabel.text = "Sin Entradas";
            }

            if (entryTextLabel != null)
            {
                entryTextLabel.text = "Aún no has desbloqueado ninguna entrada del diario.\n\nCompleta niveles para desbloquear nuevas páginas de tu historia.";
            }

            if (pageIndicatorLabel != null)
            {
                pageIndicatorLabel.text = "";
            }
        }

        /// <summary>
        /// Navega a la página anterior.
        /// </summary>
        private void ShowPreviousPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                ShowCurrentPage();
                Debug.Log($"[DiaryUI] Página anterior: {currentPageIndex + 1}/{unlockedEntries.Count}");
            }
        }

        /// <summary>
        /// Navega a la página siguiente.
        /// </summary>
        private void ShowNextPage()
        {
            if (currentPageIndex < unlockedEntries.Count - 1)
            {
                currentPageIndex++;
                ShowCurrentPage();
                Debug.Log($"[DiaryUI] Página siguiente: {currentPageIndex + 1}/{unlockedEntries.Count}");
            }
        }

        /// <summary>
        /// Actualiza el indicador de página (ej: "1 / 5").
        /// </summary>
        private void UpdatePageIndicator()
        {
            if (pageIndicatorLabel == null) return;

            if (unlockedEntries.Count == 0)
            {
                pageIndicatorLabel.text = "";
            }
            else
            {
                pageIndicatorLabel.text = $"Página {currentPageIndex + 1} / {unlockedEntries.Count}";
            }
        }

        /// <summary>
        /// Actualiza el estado (habilitado/deshabilitado) de los botones de navegación.
        /// </summary>
        private void UpdateNavigationButtons()
        {
            if (prevButton != null)
            {
                prevButton.SetEnabled(currentPageIndex > 0);
            }

            if (nextButton != null)
            {
                nextButton.SetEnabled(currentPageIndex < unlockedEntries.Count - 1);
            }
        }

        #endregion

        #region Entry Display

        /// <summary>
        /// Muestra el contenido completo de una entrada.
        /// </summary>
        private void DisplayEntry(DiaryEntry entry)
        {
            if (entry == null)
            {
                Debug.LogWarning("[DiaryUI] Entrada es null, no se puede mostrar.");
                return;
            }

            // Actualizar título
            if (entryTitleLabel != null)
            {
                entryTitleLabel.text = $"Nivel {entry.level + 1}: {entry.title}";
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
                LoadUnlockedEntries();
                ShowCurrentPage();
            }
        }

        #endregion
    }
}
