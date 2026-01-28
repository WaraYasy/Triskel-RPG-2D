using UnityEngine;
using UnityEngine.UIElements;
using Triskel.Core;
using System.Collections.Generic;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del menu de ajustes.
    /// Conecta la UI con el SettingsManager.
    /// </summary>
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
        
        // Elementos decorativos
        private VisualElement themeSection; 

        private void OnEnable()
        {
            InitializeSettings();
            RefreshUI();
        }

        private void OnDisable()
        {
            UnregisterEvents();
        }

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
            themeSection = root.Q<VisualElement>("DialogueFontSizeSection");

            // Configurar Dropdown
            if (fontSizeDropdown != null)
            {
                fontSizeDropdown.choices = new List<string> { "Normal", "Grande" };
            }

            RegisterEvents();

            // Ocultar inicialmente
            Hide();
        }

        private void RegisterEvents()
        {
            if (musicSlider != null) musicSlider.RegisterValueChangedCallback(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.RegisterValueChangedCallback(OnSFXSliderChanged);
            if (fontSizeDropdown != null) fontSizeDropdown.RegisterValueChangedCallback(OnFontSizeChanged);
            if (backButton != null) backButton.clicked += OnBackClicked;
        }

        private void UnregisterEvents()
        {
            if (musicSlider != null) musicSlider.UnregisterValueChangedCallback(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.UnregisterValueChangedCallback(OnSFXSliderChanged);
            if (fontSizeDropdown != null) fontSizeDropdown.UnregisterValueChangedCallback(OnFontSizeChanged);
            if (backButton != null) backButton.clicked -= OnBackClicked;
        }

        public void Show()
        {
            if (settingsOverlay != null)
                settingsOverlay.style.display = DisplayStyle.Flex;
            
            RefreshUI();
        }

        public void Hide()
        {
            if (settingsOverlay != null)
                settingsOverlay.style.display = DisplayStyle.None;
        }

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

        private void OnMusicSliderChanged(ChangeEvent<float> evt)
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetMusicVolume(evt.newValue / 100f);
        }

        private void OnSFXSliderChanged(ChangeEvent<float> evt)
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetSFXVolume(evt.newValue / 100f);
        }

        private void OnFontSizeChanged(ChangeEvent<string> evt)
        {
            if (SettingsManager.Instance != null)
            {
                bool largeText = evt.newValue == "Grande";
                SettingsManager.Instance.SetLargeText(largeText);
            }
        }

        private void OnBackClicked()
        {
            Hide();
            if (pauseController != null && pauseController.IsPaused)
                pauseController.Show();
        }

        #endregion
    }
}
