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

        // Elementos UI
        private VisualElement settingsOverlay;
        private Slider musicSlider;
        private Slider sfxSlider;
        private DropdownField fontSizeDropdown;
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
        }

        private void OnDisable()
        {
            UnregisterEvents();
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
            backButton = root.Q<Button>("BackButton");

            // Configurar Dropdown
            if (fontSizeDropdown != null)
            {
                fontSizeDropdown.choices = new List<string> { "Normal", "Grande" };
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
