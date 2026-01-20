using UnityEngine;
using UnityEngine.UIElements;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del menu de ajustes.
    /// Maneja volumen de musica, efectos y tema visual.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument settingsDocument;
        [SerializeField] private PauseController pauseController;

        [Header("Audio")]
        [SerializeField] private string musicVolumeKey = "MusicVolume";
        [SerializeField] private string sfxVolumeKey = "SFXVolume";
        [SerializeField] private float defaultVolume = 80f;

        [Header("Tema")]
        [SerializeField] private string themeKey = "DarkMode";
        [SerializeField] private bool defaultDarkMode = true;

        // Elementos UI
        private VisualElement settingsOverlay;
        private Slider musicSlider;
        private Slider sfxSlider;
        private Toggle themeToggle;
        private Button backButton;

        // Estado
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public bool IsDarkMode { get; private set; }

        // Eventos
        public event System.Action<float> OnMusicVolumeChanged;
        public event System.Action<float> OnSFXVolumeChanged;
        public event System.Action<bool> OnThemeChanged;

        private void OnEnable()
        {
            InitializeSettings();
            LoadSettings();
        }

        private void OnDisable()
        {
            if (musicSlider != null) musicSlider.UnregisterValueChangedCallback(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.UnregisterValueChangedCallback(OnSFXSliderChanged);
            if (themeToggle != null) themeToggle.UnregisterValueChangedCallback(OnThemeToggleChanged);
            if (backButton != null) backButton.clicked -= OnBackClicked;
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

            // Obtener referencias
            settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
            musicSlider = root.Q<Slider>("MusicSlider");
            sfxSlider = root.Q<Slider>("SFXSlider");
            themeToggle = root.Q<Toggle>("ThemeToggle");
            backButton = root.Q<Button>("BackButton");

            // Configurar eventos
            if (musicSlider != null) musicSlider.RegisterValueChangedCallback(OnMusicSliderChanged);
            if (sfxSlider != null) sfxSlider.RegisterValueChangedCallback(OnSFXSliderChanged);
            if (themeToggle != null) themeToggle.RegisterValueChangedCallback(OnThemeToggleChanged);
            if (backButton != null) backButton.clicked += OnBackClicked;

            // Ocultar inicialmente
            Hide();
        }

        #region Public Methods

        public void Show()
        {
            if (settingsOverlay != null)
                settingsOverlay.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (settingsOverlay != null)
                settingsOverlay.style.display = DisplayStyle.None;
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp(volume, 0f, 100f);

            if (musicSlider != null)
                musicSlider.SetValueWithoutNotify(MusicVolume);

            ApplyMusicVolume();
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp(volume, 0f, 100f);

            if (sfxSlider != null)
                sfxSlider.SetValueWithoutNotify(SFXVolume);

            ApplySFXVolume();
            SaveSettings();
        }

        public void SetDarkMode(bool darkMode)
        {
            IsDarkMode = darkMode;

            if (themeToggle != null)
                themeToggle.SetValueWithoutNotify(IsDarkMode);

            ApplyTheme();
            SaveSettings();
        }

        #endregion

        #region Event Handlers

        private void OnMusicSliderChanged(ChangeEvent<float> evt)
        {
            MusicVolume = evt.newValue;
            ApplyMusicVolume();
            SaveSettings();

            Debug.Log($"[SettingsController] Volumen musica: {MusicVolume}%");
        }

        private void OnSFXSliderChanged(ChangeEvent<float> evt)
        {
            SFXVolume = evt.newValue;
            ApplySFXVolume();
            SaveSettings();

            Debug.Log($"[SettingsController] Volumen SFX: {SFXVolume}%");
        }

        private void OnThemeToggleChanged(ChangeEvent<bool> evt)
        {
            IsDarkMode = evt.newValue;
            ApplyTheme();
            SaveSettings();

            Debug.Log($"[SettingsController] Modo oscuro: {IsDarkMode}");
        }

        private void OnBackClicked()
        {
            Debug.Log("[SettingsController] Volviendo...");
            Hide();

            // Mostrar menu de pausa si existe
            if (pauseController != null && pauseController.IsPaused)
                pauseController.Show();
        }

        #endregion

        #region Apply Settings

        private void ApplyMusicVolume()
        {
            // TODO: Conectar con tu MusicManager
            // Ejemplo: MusicManager.Instance?.SetVolume(MusicVolume / 100f);

            OnMusicVolumeChanged?.Invoke(MusicVolume);
        }

        private void ApplySFXVolume()
        {
            // TODO: Conectar con tu SFXManager o AudioManager
            // Ejemplo: AudioManager.Instance?.SetSFXVolume(SFXVolume / 100f);

            OnSFXVolumeChanged?.Invoke(SFXVolume);
        }

        private void ApplyTheme()
        {
            // TODO: Conectar con tu DialogueThemeManager o sistema de temas
            // Ejemplo: DialogueThemeManager.Instance?.SetTheme(IsDarkMode ? DialogueTheme.Oscuro : DialogueTheme.Claro);

            OnThemeChanged?.Invoke(IsDarkMode);
        }

        #endregion

        #region Persistence

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(musicVolumeKey, MusicVolume);
            PlayerPrefs.SetFloat(sfxVolumeKey, SFXVolume);
            PlayerPrefs.SetInt(themeKey, IsDarkMode ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadSettings()
        {
            // Cargar valores guardados o usar defaults
            MusicVolume = PlayerPrefs.GetFloat(musicVolumeKey, defaultVolume);
            SFXVolume = PlayerPrefs.GetFloat(sfxVolumeKey, defaultVolume);
            IsDarkMode = PlayerPrefs.GetInt(themeKey, defaultDarkMode ? 1 : 0) == 1;

            // Actualizar UI
            if (musicSlider != null) musicSlider.SetValueWithoutNotify(MusicVolume);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(SFXVolume);
            if (themeToggle != null) themeToggle.SetValueWithoutNotify(IsDarkMode);

            // Aplicar settings
            ApplyMusicVolume();
            ApplySFXVolume();
            ApplyTheme();

            Debug.Log($"[SettingsController] Settings cargados - Music: {MusicVolume}%, SFX: {SFXVolume}%, DarkMode: {IsDarkMode}");
        }

        #endregion
    }
}
