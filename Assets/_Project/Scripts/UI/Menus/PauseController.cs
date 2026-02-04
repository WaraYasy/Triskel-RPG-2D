// =======================================================================================
// Triskel RPG 2D - Pause Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador del menú de pausa del juego. Gestiona la pausa del tiempo
//              (Time.timeScale), muestra/oculta el menú y proporciona opciones de
//              continuar, reiniciar, ajustes y salir al menú principal.
// =======================================================================================

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Triskel.API;
using Triskel.Core;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del menú de pausa del juego.
    /// </summary>
    /// <remarks>
    /// Este controlador gestiona el estado de pausa del juego usando Time.timeScale.
    /// Detecta las teclas ESC y P para pausar/despausar, y coordina con el SettingsController
    /// para mostrar el menú de ajustes cuando sea necesario.
    ///
    /// Eventos disponibles: OnPause, OnResume
    /// </remarks>
    public class PauseController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument pauseDocument;
        [SerializeField] private SettingsController settingsController;
        [SerializeField] private ControlsDisplayController controlsDisplay;
        [SerializeField] private GameConstants gameConstants;
        [SerializeField] private Font pixelFont;
        [SerializeField] private Font dyslexicFont;

        [Header("Escenas")]
        [SerializeField] private string mainMenuSceneName = "Home";

        // Elementos UI - Pause
        private VisualElement pauseOverlay;
        private Button continueButton;
        private Button restartButton;
        private Button settingsButton;
        private Button helpButton;
        private Button quitButton;

        // Estado
        private bool isPaused;
        /// <summary>
        /// Indica si el juego está actualmente pausado.
        /// </summary>
        public bool IsPaused => isPaused;

        // Eventos
        /// <summary>
        /// Evento que se dispara cuando el juego se pausa.
        /// </summary>
        public event System.Action OnPause;
        /// <summary>
        /// Evento que se dispara cuando el juego se reanuda.
        /// </summary>
        public event System.Action OnResume;

        private void OnEnable()
        {
            // Buscar SettingsController si no está asignado (útil con UI persistente)
            if (settingsController == null)
            {
                // Buscar en el padre (UI root)
                var uiRoot = transform.parent;
                if (uiRoot != null)
                    settingsController = uiRoot.GetComponentInChildren<SettingsController>(true);
            }

            InitializePauseMenu();

            // Suscribirse a eventos de SettingsManager
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontChanged += ApplyFontToPauseUI;
                SettingsManager.Instance.OnFontSizeChanged += ApplyFontSizeToPauseUI;
            }
        }

        private void OnDisable()
        {
            if (continueButton != null) continueButton.clicked -= OnContinueClicked;
            if (restartButton != null) restartButton.clicked -= OnRestartClicked;
            if (settingsButton != null) settingsButton.clicked -= OnSettingsClicked;
            if (quitButton != null) quitButton.clicked -= OnQuitClicked;

            // Desuscribirse de eventos de SettingsManager
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontChanged -= ApplyFontToPauseUI;
                SettingsManager.Instance.OnFontSizeChanged -= ApplyFontSizeToPauseUI;
            }
        }

        private void Update()
        {
            // Detectar tecla Escape o P para pausar/despausar (usando nuevo Input System)
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                {
                    // Si settings está abierto, cerrar settings primero
                    if (settingsController != null && settingsController.IsVisible)
                    {
                        settingsController.Hide();
                        Show();
                        return;
                    }

                    // Si no, toggle pausa normal
                    if (isPaused)
                        Resume();
                    else
                        Pause();
                }
            }
        }

        /// <summary>
        /// Inicializa el menú de pausa obteniendo referencias a los elementos UI y registrando eventos.
        /// </summary>
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
            if (root == null)
            {
                Debug.LogWarning("[PauseController] rootVisualElement aun no esta listo");
                return;
            }

            // Obtener referencias de pausa
            pauseOverlay = root.Q<VisualElement>("PauseOverlay");
            continueButton = root.Q<Button>("ContinueButton");
            restartButton = root.Q<Button>("RestartButton");
            settingsButton = root.Q<Button>("SettingsButton");
            helpButton = root.Q<Button>("HelpButton");
            quitButton = root.Q<Button>("QuitButton");

            // Configurar eventos de pausa
            if (continueButton != null) continueButton.clicked += OnContinueClicked;
            if (restartButton != null) restartButton.clicked += OnRestartClicked;
            if (settingsButton != null) settingsButton.clicked += OnSettingsClicked;
            if (helpButton != null) helpButton.clicked += OnHelpClicked;
            if (quitButton != null) quitButton.clicked += OnQuitClicked;

            // Ocultar inicialmente
            Hide();

            // Aplicar fuente y tamaño inicial
            ApplyFontToPauseUI(SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont);
            ApplyFontSizeToPauseUI(SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText);
        }

        private void ApplyFontToPauseUI(bool useDyslexic)
        {
            if (pauseOverlay == null) return;

            Font activeFont = (useDyslexic && dyslexicFont != null) ? dyslexicFont : pixelFont;

            if (activeFont == null)
            {
                Debug.LogWarning($"[PauseController] ⚠️ No hay fuente asignada - pixelFont: {(pixelFont != null ? pixelFont.name : "NULL")}, dyslexicFont: {(dyslexicFont != null ? dyslexicFont.name : "NULL")}");
                return;
            }

            Debug.Log($"[PauseController] Aplicando fuente: {activeFont.name} (Dislexia: {useDyslexic})");

            var fontDef = FontDefinition.FromFont(activeFont);
            pauseOverlay.Query<Label>().ForEach(label => label.style.unityFontDefinition = fontDef);
            pauseOverlay.Query<Button>().ForEach(btn => btn.style.unityFontDefinition = fontDef);

            // Reaplicar tamaño porque cada fuente tiene tamaños diferentes
            ApplyFontSizeToPauseUI(SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText);
        }

        private void ApplyFontSizeToPauseUI(bool useLarge)
        {
            if (pauseOverlay == null)
            {
                Debug.LogWarning("[PauseController] pauseOverlay es null");
                return;
            }

            if (gameConstants == null)
            {
                Debug.LogWarning("[PauseController] ⚠️ GameConstants no asignado - no se puede aplicar tamaño de fuente");
                return;
            }

            bool dyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;

            float fontSize = dyslexic
                ? (useLarge ? gameConstants.menuFontSizeLargeDyslexic : gameConstants.menuFontSizeNormalDyslexic)
                : (useLarge ? gameConstants.menuFontSizeLarge : gameConstants.menuFontSizeNormal);

            Debug.Log($"[PauseController] Aplicando tamaño: {fontSize}px (Grande: {useLarge}, Dislexia: {dyslexic})");

            pauseOverlay.Query<Label>().ForEach(label => label.style.fontSize = fontSize);
            pauseOverlay.Query<Button>().ForEach(btn => btn.style.fontSize = fontSize);
        }

        #region Public Methods

        /// <summary>
        /// Pausa el juego estableciendo Time.timeScale a 0 y muestra el menú de pausa.
        /// </summary>
        /// <remarks>
        /// Este método puede ser llamado desde botones Unity UI (como el botón de pausa móvil)
        /// o desde código. Dispara el evento OnPause.
        /// </remarks>
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

        /// <summary>
        /// Reanuda el juego restaurando Time.timeScale a 1 y oculta el menú de pausa.
        /// </summary>
        /// <remarks>
        /// También oculta el menú de ajustes si está abierto. Dispara el evento OnResume.
        /// </remarks>
        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;
            Hide();

            // Ocultar settings si está abierto
            if (settingsController != null)
                settingsController.Hide();

            OnResume?.Invoke();

            Debug.Log("[PauseController] Juego reanudado");
        }

        /// <summary>
        /// Muestra el menú de pausa sin modificar el estado de Time.timeScale.
        /// </summary>
        public void Show()
        {
            if (pauseOverlay != null)
                pauseOverlay.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Oculta el menú de pausa sin modificar el estado de Time.timeScale.
        /// </summary>
        public void Hide()
        {
            if (pauseOverlay != null)
                pauseOverlay.style.display = DisplayStyle.None;
        }

        #endregion

        #region Button Handlers

        /// <summary>
        /// Callback cuando se hace clic en el botón "Continuar".
        /// Reanuda el juego llamando a Resume().
        /// </summary>
        private void OnContinueClicked()
        {
            // Prevenir doble-click
            if (!isPaused) return;

            Resume();
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Reiniciar".
        /// Guarda el progreso actual y recarga la escena actual.
        /// </summary>
        private void OnRestartClicked()
        {
            Debug.Log("[PauseController] Reiniciando nivel...");

            // Resetear tracking del nivel antes de reiniciar
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SaveGame();

                // Resetear tiempo y muertes del nivel (mantiene totalDeaths)
                GameManager.Instance.GetAPITracker()?.ResetLevelTracking();

                // Eliminar la reliquia del nivel actual (se pierde al reiniciar)
                GameManager.Instance.RemoveCurrentLevelRelic();
            }

            // Ocultar ventana de pausa
            Hide();

            // Ocultar settings si está abierto
            if (settingsController != null)
                settingsController.Hide();

            // Restaurar timeScale antes de recargar
            Time.timeScale = 1f;
            isPaused = false;

            // Recargar escena actual
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Ajustes".
        /// Oculta el menú de pausa y muestra el menú de ajustes.
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[PauseController] Abriendo ajustes...");

            if (settingsController != null)
            {
                Hide();
                settingsController.Show();
            }
            else
            {
                Debug.LogWarning("[PauseController] SettingsController no asignado");
            }
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Ayuda".
        /// Oculta el menú de pausa y muestra la pantalla de controles.
        /// </summary>
        private void OnHelpClicked()
        {
            Debug.Log("[PauseController] Abriendo ayuda/controles...");

            // Buscar ControlsDisplayController si no está asignado
            if (controlsDisplay == null)
            {
                controlsDisplay = FindFirstObjectByType<ControlsDisplayController>();
            }

            if (controlsDisplay != null)
            {
                Hide();
                controlsDisplay.Show();
            }
            else
            {
                Debug.LogWarning("[PauseController] ControlsDisplayController no encontrado en la escena");
            }
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Salir".
        /// Guarda el progreso, restablece Time.timeScale y carga el menú principal.
        /// </summary>
        private void OnQuitClicked()
        {
            Debug.Log("[PauseController] Volviendo al menu principal...");

            // Guardar partida antes de salir
            if (GameManager.Instance != null)
                GameManager.Instance.SaveGame();

            // Ocultar ventanas
            Hide();
            if (settingsController != null)
                settingsController.Hide();

            // Restaurar timeScale antes de cambiar de escena
            Time.timeScale = 1f;
            isPaused = false;

            // Cargar menu principal
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        #endregion
    }
}
