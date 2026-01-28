using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using Triskel.Core;
using Button = UnityEngine.UIElements.Button;

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

        [Header("Tamaño de Texto")]
        [SerializeField] private float normalTitleSize = 50f;
        [SerializeField] private float largeTitleSize = 55f;
        [SerializeField] private float normalTextSize = 35f;
        [SerializeField] private float largeTextSize = 45f;

        // Botón móvil del diario
        private UnityEngine.UI.Button mobileDiaryButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();

            // Buscar el botón del diario en los controles móviles
            FindMobileDiaryButton();
        }

        private void OnEnable()
        {
            // Intentar inicializar inmediatamente para evitar que la UI aparezca un segundo (parpadeo)
            if (!isInitialized)
            {
                if (uiDocument == null) uiDocument = GetComponent<UIDocument>();

                if (uiDocument != null && uiDocument.rootVisualElement != null)
                {
                    InitializeUI();
                }
                else
                {
                    // Fallback: Si por alguna razón no está lista, esperar lo mínimo posible
                    Invoke(nameof(InitializeUI), 0.01f);
                }
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged += ApplyFontSize;
            }
        }

        private void OnDisable()
        {
            // Limpiar eventos al desactivar
            UnregisterEvents();

            // Desconectar botón móvil
            if (mobileDiaryButton != null)
            {
                mobileDiaryButton.onClick.RemoveListener(OnMobileDiaryButtonClicked);
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged -= ApplyFontSize;
            }
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
            isInitialized = true;
            Debug.Log("[DiaryUI] UI inicializada correctamente.");
            
            // Aplicar tamaño inicial
            if (SettingsManager.Instance != null)
            {
                ApplyFontSize(SettingsManager.Instance.UseLargeText);
            }

        }

        private void Start()
        {
             // Retry subscription if it failed in OnEnable (Race Condition fix)
            if (SettingsManager.Instance != null)
            {
                 // Asegurar no suscribirse doble
                 SettingsManager.Instance.OnFontSizeChanged -= ApplyFontSize;
                 SettingsManager.Instance.OnFontSizeChanged += ApplyFontSize;
                 
                 // Aplicar inicial
                 ApplyFontSize(SettingsManager.Instance.UseLargeText);
            }
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
            // Detectar tecla J para abrir/cerrar diario (usando nuevo Input System)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.jKey.wasPressedThisFrame)
                {
                    TogglePanel();
                }
            }
        }

        #region Mobile Controls

        /// <summary>
        /// Busca y conecta el botón del diario en los controles móviles.
        /// Busca por nombre: "DiaryButton", "ButtonDiary", "BtnDiary" o similar.
        /// </summary>
        private void FindMobileDiaryButton()
        {
            // Buscar todos los botones en la escena
            UnityEngine.UI.Button[] allButtons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);

            foreach (var button in allButtons)
            {
                // Buscar por nombre (case insensitive)
                string buttonName = button.gameObject.name.ToLower();

                if (buttonName.Contains("diary") || buttonName.Contains("diario"))
                {
                    mobileDiaryButton = button;
                    mobileDiaryButton.onClick.AddListener(OnMobileDiaryButtonClicked);
                    Debug.Log($"[DiaryUI] Botón móvil del diario encontrado y conectado: {button.gameObject.name}");
                    return;
                }
            }

            Debug.LogWarning("[DiaryUI] No se encontró botón de diario en los controles móviles. Asegúrate de que el botón tenga 'Diary' o 'Diario' en su nombre.");
        }

        /// <summary>
        /// Se ejecuta cuando se presiona el botón móvil del diario.
        /// </summary>
        private void OnMobileDiaryButtonClicked()
        {
            Debug.Log("[DiaryUI] Botón móvil del diario presionado");
            TogglePanel();
        }

        #endregion

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
        #region Font Size Control

        private void ApplyFontSize(bool large)
        {
            if (entryTitleLabel != null)
            {
                entryTitleLabel.style.fontSize = new Length(large ? largeTitleSize : normalTitleSize, LengthUnit.Pixel);
            }

            if (entryTextLabel != null)
            {
                entryTextLabel.style.fontSize = new Length(large ? largeTextSize : normalTextSize, LengthUnit.Pixel);
            }

            Debug.Log($"[DiaryUI] Tamaño de fuente aplicado: {(large ? "Grande" : "Normal")}");
        }

        #endregion
    }
}
