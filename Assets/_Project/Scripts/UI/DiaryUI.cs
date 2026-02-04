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
        private VisualElement scrollIndicator;

        // Estado
        private bool isPanelOpen = false;
        private List<DiaryEntry> unlockedEntries = new List<DiaryEntry>();
        private int currentPageIndex = 0;
        private bool isInitialized = false;

        [Header("Referencias")]
        [SerializeField] private GameConstants gameConstants;
        [SerializeField] private Font pixelFont;
        [SerializeField] private Font dyslexicFont;

        // Botón móvil del diario
        private UnityEngine.UI.Button mobileDiaryButton;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();

            // Buscar el botón del diario en los controles móviles (incluso si están inactivos al inicio)
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
                SettingsManager.Instance.OnFontChanged += ApplyFont;
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
                SettingsManager.Instance.OnFontChanged -= ApplyFont;
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
            scrollIndicator = root.Q<VisualElement>("ScrollIndicator");

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
            
            // Aplicar fuente y tamaño inicial
            if (SettingsManager.Instance != null)
            {
                ApplyFont(SettingsManager.Instance.UseDyslexicFont);
                ApplyFontSize(SettingsManager.Instance.UseLargeText);
            }

        }

        private void Start()
        {
             // Reintentar buscar el botón móvil si no se encontró en Awake
             if (mobileDiaryButton == null)
             {
                 FindMobileDiaryButton();
             }

             // Retry subscription if it failed in OnEnable (Race Condition fix)
            if (SettingsManager.Instance != null)
            {
                 // Asegurar no suscribirse doble
                 SettingsManager.Instance.OnFontSizeChanged -= ApplyFontSize;
                 SettingsManager.Instance.OnFontSizeChanged += ApplyFontSize;
                 SettingsManager.Instance.OnFontChanged -= ApplyFont;
                 SettingsManager.Instance.OnFontChanged += ApplyFont;

                 // Aplicar inicial
                 ApplyFont(SettingsManager.Instance.UseDyslexicFont);
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

            if (scrollIndicator != null)
            {
                scrollIndicator.RegisterCallback<ClickEvent>(OnScrollIndicatorClicked);
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

            if (scrollIndicator != null)
            {
                scrollIndicator.UnregisterCallback<ClickEvent>(OnScrollIndicatorClicked);
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
            // Buscar todos los botones en la escena, INCLUYENDO los inactivos
            // "DiaryButton", "ButtonDiary", "BtnDiary" o similar en su nombre.
            UnityEngine.UI.Button[] allButtons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var button in allButtons)
            {
                // Buscar por nombre (case insensitive)
                string buttonName = button.gameObject.name.ToLower();

                if (buttonName.Contains("diary") || buttonName.Contains("diario"))
                {
                    mobileDiaryButton = button;
                    mobileDiaryButton.onClick.RemoveListener(OnMobileDiaryButtonClicked); // Prevenir duplicados
                    mobileDiaryButton.onClick.AddListener(OnMobileDiaryButtonClicked);
                    Debug.Log($"[DiaryUI] Botón móvil del diario encontrado y conectado: {button.gameObject.name}");
                    return;
                }
            }

            // No emitir warning en editor si no estamos emulando móvil, para no saturar la consola
            if (Application.isMobilePlatform || Debug.isDebugBuild)
            {
                Debug.LogWarning("[DiaryUI] No se encontró botón de diario en los controles móviles (buscando 'Diary' o 'Diario').");
            }
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
                entryTitleLabel.text = "¿Qué está pasando?";
            }

            if (entryTextLabel != null)
            {
                entryTextLabel.text = "Querido diario, aun no conozco este lugar. Tan pronto descubra que hay detrás de estas puertas te contaré mis descubriemientos...";
            }

            if (pageIndicatorLabel != null)
            {
                pageIndicatorLabel.text = "";
            }

            // Actualizar indicador de scroll
            UpdateScrollIndicator();
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

            // Actualizar indicador de scroll
            UpdateScrollIndicator();

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
        #region Font Control

        private void ApplyFont(bool useDyslexic)
        {
            Font activeFont = (useDyslexic && dyslexicFont != null) ? dyslexicFont : pixelFont;
            if (activeFont == null) return;

            var fontDef = FontDefinition.FromFont(activeFont);

            if (entryTitleLabel != null)
                entryTitleLabel.style.unityFontDefinition = fontDef;

            if (entryTextLabel != null)
                entryTextLabel.style.unityFontDefinition = fontDef;

            if (pageIndicatorLabel != null)
                pageIndicatorLabel.style.unityFontDefinition = fontDef;

            if (prevButton != null)
                prevButton.style.unityFontDefinition = fontDef;

            if (nextButton != null)
                nextButton.style.unityFontDefinition = fontDef;

            // Re-aplicar tamaños para usar los valores correctos según la fuente activa
            if (SettingsManager.Instance != null)
                ApplyFontSize(SettingsManager.Instance.UseLargeText);
        }

        private void ApplyFontSize(bool large)
        {
            if (gameConstants == null) return;

            bool dyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;

            float titleSize = dyslexic
                ? (large ? gameConstants.diaryTitleSizeLargeDyslexic : gameConstants.diaryTitleSizeNormalDyslexic)
                : (large ? gameConstants.diaryTitleSizeLarge : gameConstants.diaryTitleSizeNormal);

            float textSize = dyslexic
                ? (large ? gameConstants.diaryTextSizeLargeDyslexic : gameConstants.diaryTextSizeNormalDyslexic)
                : (large ? gameConstants.diaryTextSizeLarge : gameConstants.diaryTextSizeNormal);

            if (entryTitleLabel != null)
                entryTitleLabel.style.fontSize = new Length(titleSize, LengthUnit.Pixel);

            if (entryTextLabel != null)
                entryTextLabel.style.fontSize = new Length(textSize, LengthUnit.Pixel);

            Debug.Log($"[DiaryUI] Tamaño de fuente aplicado: {(large ? "Grande" : "Normal")} ({(dyslexic ? "Dislexia" : "Pixelada")})");
        }

        #endregion

        #region Scroll Indicator

        /// <summary>
        /// Actualiza la visibilidad del indicador de scroll (flecha).
        /// Solo se muestra si hay contenido scrollable.
        /// </summary>
        private void UpdateScrollIndicator()
        {
            if (scrollIndicator == null || entryTextScrollView == null || entryTextLabel == null)
                return;

            // Esperar un frame para que el layout se calcule
            entryTextScrollView.schedule.Execute(() =>
            {
                // Verificar si el contenido es más alto que el contenedor
                float contentHeight = entryTextLabel.layout.height;
                float viewportHeight = entryTextScrollView.contentViewport.layout.height;
                bool isScrollable = contentHeight > viewportHeight;

                // Mostrar/ocultar flecha
                scrollIndicator.style.display = isScrollable ? DisplayStyle.Flex : DisplayStyle.None;
            });
        }

        private void OnScrollIndicatorClicked(ClickEvent evt)
        {
            if (entryTextScrollView != null)
            {
                // Hacer scroll hacia abajo (1 página)
                float scrollAmount = entryTextScrollView.contentViewport.layout.height * 0.8f;
                entryTextScrollView.scrollOffset = new Vector2(0, entryTextScrollView.scrollOffset.y + scrollAmount);
            }
        }

        #endregion
    }
}
