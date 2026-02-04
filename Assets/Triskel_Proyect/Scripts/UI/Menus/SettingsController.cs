// =======================================================================================
// Triskel RPG 2D - Settings Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador del menú de ajustes que conecta la interfaz de usuario
//              (UI Toolkit) con el SettingsManager. Gestiona los controles de volumen
//              de música, efectos de sonido y tamaño de fuente para diálogos.
// =======================================================================================

using UnityEngine;
using UnityEngine.UIElements;
using Triskel.Core;
using System.Collections.Generic;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del menú de ajustes del juego.
    /// </summary>
    /// <remarks>
    /// Este controlador gestiona la interfaz de configuración del juego, permitiendo al jugador
    /// ajustar el volumen de música, efectos de sonido y tamaño de fuente. Todos los cambios
    /// se persisten automáticamente a través del SettingsManager.
    ///
    /// Debe añadirse como componente al GameObject que contiene el UIDocument de Settings.
    /// </remarks>
    public class SettingsController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument settingsDocument;
        [SerializeField] private PauseController pauseController;
        [SerializeField] private GameConstants gameConstants;
        [SerializeField] private Font pixelFont;
        [SerializeField] private Font dyslexicFont;

        // Elementos UI
        private VisualElement settingsOverlay;
        private Slider musicSlider;
        private Slider sfxSlider;
        private DropdownField fontSizeDropdown;
        private DropdownField fontTypeDropdown;
        private Button backButton;

        // Estado
        /// <summary>
        /// Indica si el menú de ajustes está actualmente visible.
        /// </summary>
        public bool IsVisible { get; private set; } 

        private void OnEnable()
        {
            // Buscar PauseController si no está asignado (útil con UI persistente)
            if (pauseController == null)
            {
                // Buscar en el padre (UI root)
                var uiRoot = transform.parent;
                if (uiRoot != null)
                    pauseController = uiRoot.GetComponentInChildren<PauseController>(true);
            }

            InitializeSettings();
            RefreshUI();

            // Suscribirse a eventos de SettingsManager para actualizar la UI del menú
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontChanged += OnMenuFontChanged;
                SettingsManager.Instance.OnFontSizeChanged += OnMenuFontSizeChanged;
            }
        }

        private void OnDisable()
        {
            UnregisterEvents();

            // Desuscribirse de eventos de SettingsManager
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontChanged -= OnMenuFontChanged;
                SettingsManager.Instance.OnFontSizeChanged -= OnMenuFontSizeChanged;
            }
        }

        /// <summary>
        /// Inicializa el menú de ajustes obteniendo referencias a los elementos UI y registrando eventos.
        /// </summary>
        private void InitializeSettings()
        {
            if (settingsDocument == null)
                settingsDocument = GetComponent<UIDocument>();

            if (settingsDocument == null)
            {
                Debug.LogError("[SettingsController] UIDocument no asignado");
                return;
            }

            var root = settingsDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogWarning("[SettingsController] rootVisualElement aun no esta listo");
                return;
            }

            // Obtener referencias
            settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
            musicSlider = root.Q<Slider>("MusicSlider");
            sfxSlider = root.Q<Slider>("SFXSlider");
            fontSizeDropdown = root.Q<DropdownField>("DialogueFontSizeDropdown");
            fontTypeDropdown = root.Q<DropdownField>("FontTypeDropdown");
            backButton = root.Q<Button>("BackButton");

            // Configurar Dropdowns
            if (fontSizeDropdown != null)
            {
                fontSizeDropdown.choices = new List<string> { "Normal", "Grande" };
            }

            if (fontTypeDropdown != null)
            {
                fontTypeDropdown.choices = new List<string> { "Pixelada", "Dislexia" };
            }

            RegisterEvents();

            // Ocultar inicialmente
            Hide();
        }

        /// <summary>
        /// Registra los callbacks para los eventos de los controles UI (sliders, dropdown, botones).
        /// </summary>
        private void RegisterEvents()
        {
            if (musicSlider != null) musicSlider.RegisterValueChangedCallback(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.RegisterValueChangedCallback(OnSFXSliderChanged);
            if (fontSizeDropdown != null) fontSizeDropdown.RegisterValueChangedCallback(OnFontSizeChanged);
            if (fontTypeDropdown != null) fontTypeDropdown.RegisterValueChangedCallback(OnFontTypeChanged);
            if (backButton != null) backButton.clicked += OnBackClicked;
        }

        /// <summary>
        /// Desregistra los callbacks de eventos para evitar memory leaks al deshabilitar el componente.
        /// </summary>
        private void UnregisterEvents()
        {
            if (musicSlider != null) musicSlider.UnregisterValueChangedCallback(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.UnregisterValueChangedCallback(OnSFXSliderChanged);
            if (fontSizeDropdown != null) fontSizeDropdown.UnregisterValueChangedCallback(OnFontSizeChanged);
            if (fontTypeDropdown != null) fontTypeDropdown.UnregisterValueChangedCallback(OnFontTypeChanged);
            if (backButton != null) backButton.clicked -= OnBackClicked;
        }

        /// <summary>
        /// Muestra el menú de ajustes y actualiza los valores de los controles con la configuración actual.
        /// </summary>
        public void Show()
        {
            if (settingsOverlay != null)
            {
                settingsOverlay.style.display = DisplayStyle.Flex;
                IsVisible = true;
            }

            RefreshUI();
        }

        /// <summary>
        /// Oculta el menú de ajustes y guarda la configuración en PlayerPrefs.
        /// </summary>
        public void Hide()
        {
            if (settingsOverlay != null)
            {
                settingsOverlay.style.display = DisplayStyle.None;
                IsVisible = false;
            }

            // Guardar settings al cerrar (optimización)
            if (SettingsManager.Instance != null)
                PlayerPrefs.Save();
        }

        /// <summary>
        /// Actualiza los valores de los controles UI con la configuración actual del SettingsManager.
        /// </summary>
        /// <remarks>
        /// Usa SetValueWithoutNotify para evitar triggear los callbacks y crear loops infinitos.
        /// </remarks>
        private void RefreshUI()
        {
            if (SettingsManager.Instance == null) return;

            if (musicSlider != null) 
                musicSlider.SetValueWithoutNotify(SettingsManager.Instance.MusicVolume * 100f);
            
            if (sfxSlider != null) 
                sfxSlider.SetValueWithoutNotify(SettingsManager.Instance.SFXVolume * 100f);

            if (fontSizeDropdown != null)
            {
                int index = SettingsManager.Instance.UseLargeText ? 1 : 0;
                fontSizeDropdown.index = index;
            }

            if (fontTypeDropdown != null)
            {
                fontTypeDropdown.index = SettingsManager.Instance.UseDyslexicFont ? 1 : 0;
            }

            ApplyFontToSettingsUI();
            ApplyFontSizeToSettingsUI();
        }

        /// <summary>
        /// Aplica el tamaño de fuente activo a todos los elementos de texto del overlay de Settings.
        /// </summary>
        private void ApplyFontSizeToSettingsUI()
        {
            if (settingsOverlay == null)
            {
                Debug.LogWarning("[SettingsController] settingsOverlay es null");
                return;
            }

            if (gameConstants == null)
            {
                Debug.LogWarning("[SettingsController] ⚠️ GameConstants no asignado - no se puede aplicar tamaño de fuente");
                return;
            }

            bool large = SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText;
            bool dyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;

            float fontSize = dyslexic
                ? (large ? gameConstants.menuFontSizeLargeDyslexic : gameConstants.menuFontSizeNormalDyslexic)
                : (large ? gameConstants.menuFontSizeLarge : gameConstants.menuFontSizeNormal);

            Debug.Log($"[SettingsController] Aplicando tamaño: {fontSize}px (Grande: {large}, Dislexia: {dyslexic})");

            settingsOverlay.Query<Label>().ForEach(label => label.style.fontSize = fontSize);
            settingsOverlay.Query<Button>().ForEach(btn => btn.style.fontSize = fontSize);
            settingsOverlay.Query<DropdownField>().ForEach(dd => dd.style.fontSize = fontSize);
        }

        /// <summary>
        /// Callback cuando cambia el tipo de fuente desde otro menú/UI.
        /// </summary>
        private void OnMenuFontChanged(bool useDyslexic)
        {
            ApplyFontToSettingsUI();
        }

        /// <summary>
        /// Callback cuando cambia el tamaño de fuente desde otro menú/UI.
        /// </summary>
        private void OnMenuFontSizeChanged(bool useLarge)
        {
            ApplyFontSizeToSettingsUI();
        }

        #region Event Handlers

        /// <summary>
        /// Callback cuando el slider de música cambia de valor.
        /// </summary>
        /// <param name="evt">Evento con el nuevo valor (0-100).</param>
        private void OnMusicSliderChanged(ChangeEvent<float> evt)
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetMusicVolume(evt.newValue / 100f);
        }

        /// <summary>
        /// Callback cuando el slider de efectos de sonido cambia de valor.
        /// </summary>
        /// <param name="evt">Evento con el nuevo valor (0-100).</param>
        private void OnSFXSliderChanged(ChangeEvent<float> evt)
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetSFXVolume(evt.newValue / 100f);
        }

        /// <summary>
        /// Callback cuando el dropdown de tamaño de fuente cambia de selección.
        /// </summary>
        /// <param name="evt">Evento con el nuevo valor ("Normal" o "Grande").</param>
        private void OnFontSizeChanged(ChangeEvent<string> evt)
        {
            if (SettingsManager.Instance != null)
            {
                bool largeText = evt.newValue == "Grande";
                SettingsManager.Instance.SetLargeText(largeText);
            }
        }

        /// <summary>
        /// Callback cuando el dropdown de tipo de fuente cambia de selección.
        /// </summary>
        /// <param name="evt">Evento con el nuevo valor ("Pixelada" o "Dislexia").</param>
        private void OnFontTypeChanged(ChangeEvent<string> evt)
        {
            if (SettingsManager.Instance != null)
            {
                bool dyslexic = evt.newValue == "Dislexia";
                SettingsManager.Instance.SetDyslexicFont(dyslexic);
                ApplyFontToSettingsUI();
                // Reaplicar tamaño porque cada fuente tiene tamaños diferentes
                ApplyFontSizeToSettingsUI();
            }
        }

        /// <summary>
        /// Aplica la fuente activa (Pixelada o Dislexia) a todos los elementos de texto del overlay de Settings.
        /// </summary>
        private void ApplyFontToSettingsUI()
        {
            if (settingsOverlay == null) return;

            bool useDyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;
            Font activeFont = (useDyslexic && dyslexicFont != null) ? dyslexicFont : pixelFont;

            if (activeFont == null)
            {
                Debug.LogWarning($"[SettingsController] ⚠️ No hay fuente asignada - pixelFont: {(pixelFont != null ? pixelFont.name : "NULL")}, dyslexicFont: {(dyslexicFont != null ? dyslexicFont.name : "NULL")}");
                return;
            }

            Debug.Log($"[SettingsController] Aplicando fuente: {activeFont.name} (Dislexia: {useDyslexic})");

            var fontDef = FontDefinition.FromFont(activeFont);
            settingsOverlay.Query<Label>().ForEach(label => label.style.unityFontDefinition = fontDef);
            settingsOverlay.Query<Button>().ForEach(btn => btn.style.unityFontDefinition = fontDef);
            settingsOverlay.Query<DropdownField>().ForEach(dd => dd.style.unityFontDefinition = fontDef);
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Atrás".
        /// Oculta el menú de ajustes y vuelve a mostrar el menú de pausa si está activo.
        /// </summary>
        private void OnBackClicked()
        {
            // Prevenir doble-click
            if (!IsVisible) return;

            Hide();
            if (pauseController != null && pauseController.IsPaused)
                pauseController.Show();
        }

        #endregion
    }
}
