using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del menu de pausa.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument pauseDocument;
        [SerializeField] private UIDocument settingsDocument;

        [Header("Escenas")]
        [SerializeField] private string mainMenuSceneName = "Home";

        // Elementos UI - Pause
        private VisualElement pauseOverlay;
        private Button continueButton;
        private Button restartButton;
        private Button settingsButton;
        private Button quitButton;

        // Elementos UI - Settings
        private VisualElement settingsOverlay;
        private Button backButton;

        // Estado
        private bool isPaused;
        public bool IsPaused => isPaused;

        // Eventos
        public event System.Action OnPause;
        public event System.Action OnResume;

        private void OnEnable()
        {
            InitializePauseMenu();
        }

        private void OnDisable()
        {
            if (continueButton != null) continueButton.clicked -= OnContinueClicked;
            if (restartButton != null) restartButton.clicked -= OnRestartClicked;
            if (settingsButton != null) settingsButton.clicked -= OnSettingsClicked;
            if (quitButton != null) quitButton.clicked -= OnQuitClicked;
            if (backButton != null) backButton.clicked -= OnBackFromSettingsClicked;
        }

        private void Update()
        {
            // Detectar tecla Escape o P para pausar/despausar (usando nuevo Input System)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                {
                    if (isPaused)
                        Resume();
                    else
                        Pause();
                }
            }
        }

        private void InitializePauseMenu()
        {
            if (pauseDocument == null)
                pauseDocument = GetComponent<UIDocument>();

            if (pauseDocument == null)
            {
                Debug.LogError("[PauseController] UIDocument no asignado");
                return;
            }

            var root = pauseDocument.rootVisualElement;

            // Obtener referencias de pausa
            pauseOverlay = root.Q<VisualElement>("PauseOverlay");
            continueButton = root.Q<Button>("ContinueButton");
            restartButton = root.Q<Button>("RestartButton");
            settingsButton = root.Q<Button>("SettingsButton");
            quitButton = root.Q<Button>("QuitButton");

            // Configurar eventos de pausa
            if (continueButton != null) continueButton.clicked += OnContinueClicked;
            if (restartButton != null) restartButton.clicked += OnRestartClicked;
            if (settingsButton != null) settingsButton.clicked += OnSettingsClicked;
            if (quitButton != null) quitButton.clicked += OnQuitClicked;

            // Inicializar settings
            if (settingsDocument != null)
            {
                var settingsRoot = settingsDocument.rootVisualElement;
                settingsOverlay = settingsRoot.Q<VisualElement>("SettingsOverlay");
                backButton = settingsRoot.Q<Button>("BackButton");

                if (backButton != null)
                    backButton.clicked += OnBackFromSettingsClicked;
            }

            // Ocultar inicialmente
            Hide();
            HideSettings();
        }

        #region Public Methods

        public void Pause()
        {
            if (isPaused)
            {
                Debug.LogWarning("[PauseController] Ya esta pausado");
                return;
            }

            if (pauseOverlay == null)
            {
                Debug.LogError("[PauseController] PauseOverlay es null - no se puede mostrar el menu");
                return;
            }

            isPaused = true;
            Time.timeScale = 0f;
            Show();
            OnPause?.Invoke();

            Debug.Log("[PauseController] Juego pausado - Presiona ESC o P para continuar");
        }

        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;
            Hide();
            HideSettings();
            OnResume?.Invoke();

            Debug.Log("[PauseController] Juego reanudado");
        }

        public void Show()
        {
            if (pauseOverlay != null)
                pauseOverlay.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (pauseOverlay != null)
                pauseOverlay.style.display = DisplayStyle.None;
        }

        #endregion

        #region Button Handlers

        private void OnContinueClicked()
        {
            Resume();
        }

        private void OnRestartClicked()
        {
            Debug.Log("[PauseController] Reiniciando nivel...");

            // Restaurar timeScale antes de recargar
            Time.timeScale = 1f;
            isPaused = false;

            // Recargar escena actual
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[PauseController] Abriendo ajustes...");
            ShowSettings();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[PauseController] Volviendo al menu principal...");

            // Restaurar timeScale antes de cambiar de escena
            Time.timeScale = 1f;
            isPaused = false;

            // Guardar partida antes de salir
            if (GameManager.Instance != null)
                GameManager.Instance.SaveGame();

            // Cargar menu principal
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        #endregion

        #region Settings Panel

        private void ShowSettings()
        {
            if (settingsOverlay != null)
                settingsOverlay.style.display = DisplayStyle.Flex;

            // Ocultar menu de pausa mientras se muestran settings
            Hide();
        }

        private void HideSettings()
        {
            if (settingsOverlay != null)
                settingsOverlay.style.display = DisplayStyle.None;
        }

        private void OnBackFromSettingsClicked()
        {
            Debug.Log("[PauseController] Volviendo al menu de pausa...");
            HideSettings();
            Show();
        }

        #endregion
    }
}
