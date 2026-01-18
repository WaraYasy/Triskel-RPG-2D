using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del Menu Principal.
    /// Muestra diferentes opciones segun el estado de sesion.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument mainMenuDocument;
        [SerializeField] private UIDocument loginDocument;

        [Header("Escenas")]
        [SerializeField] private string gameSceneName = "Game";

        // Elementos UI - Menu Principal
        private VisualElement mainMenuOverlay;
        private Label welcomeLabel;
        private Button loginButton;
        private Button exitButton;
        private Button continueButton;
        private Button newGameButton;
        private Button logoutButton;

        // Elementos UI - Login
        private VisualElement loginOverlay;

        private void OnEnable()
        {
            InitializeMainMenu();
            InitializeLoginPanel();

            // Suscribirse a eventos del API
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.OnLoggedIn += OnLoggedIn;
                TriskelAPIClient.Instance.OnLoggedOut += OnLoggedOut;
            }

            // Verificar estado inicial
            CheckSessionState();
        }

        private void OnDisable()
        {
            // Desuscribirse de eventos
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.OnLoggedIn -= OnLoggedIn;
                TriskelAPIClient.Instance.OnLoggedOut -= OnLoggedOut;
            }

            // Limpiar eventos de botones
            if (loginButton != null) loginButton.clicked -= OnLoginClicked;
            if (exitButton != null) exitButton.clicked -= OnExitClicked;
            if (continueButton != null) continueButton.clicked -= OnContinueClicked;
            if (newGameButton != null) newGameButton.clicked -= OnNewGameClicked;
            if (logoutButton != null) logoutButton.clicked -= OnLogoutClicked;
        }

        private void InitializeMainMenu()
        {
            if (mainMenuDocument == null)
                mainMenuDocument = GetComponent<UIDocument>();

            if (mainMenuDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocument del menu principal no asignado");
                return;
            }

            var root = mainMenuDocument.rootVisualElement;

            // Obtener referencias
            mainMenuOverlay = root.Q<VisualElement>("MainMenuOverlay");
            welcomeLabel = root.Q<Label>("WelcomeLabel");
            loginButton = root.Q<Button>("LoginButton");
            exitButton = root.Q<Button>("ExitButton");
            continueButton = root.Q<Button>("ContinueButton");
            newGameButton = root.Q<Button>("NewGameButton");
            logoutButton = root.Q<Button>("LogoutButton");

            // Configurar eventos
            if (loginButton != null) loginButton.clicked += OnLoginClicked;
            if (exitButton != null) exitButton.clicked += OnExitClicked;
            if (continueButton != null) continueButton.clicked += OnContinueClicked;
            if (newGameButton != null) newGameButton.clicked += OnNewGameClicked;
            if (logoutButton != null) logoutButton.clicked += OnLogoutClicked;
        }

        private void InitializeLoginPanel()
        {
            if (loginDocument == null)
                return;

            var root = loginDocument.rootVisualElement;
            loginOverlay = root.Q<VisualElement>("LoginOverlay");

            // Ocultar login inicialmente
            HideLogin();
        }

        private void CheckSessionState()
        {
            if (TriskelAPIClient.Instance == null)
            {
                Debug.LogWarning("[MainMenuController] TriskelAPIClient no encontrado");
                ShowLoggedOutState();
                return;
            }

            if (TriskelAPIClient.Instance.IsLoggedIn)
            {
                // Verificar que la sesion sea valida
                TriskelAPIClient.Instance.VerifySession(
                    profile => ShowLoggedInState(profile.username),
                    error => ShowLoggedOutState()
                );
            }
            else
            {
                ShowLoggedOutState();
            }
        }

        private void ShowLoggedInState(string username = null)
        {
            // Mostrar mensaje de bienvenida
            if (welcomeLabel != null)
            {
                welcomeLabel.text = string.IsNullOrEmpty(username)
                    ? "Bienvenido"
                    : $"Hola, {username}";
                welcomeLabel.style.display = DisplayStyle.Flex;
            }

            // Ocultar botones de no logueado
            SetDisplay(loginButton, false);
            SetDisplay(exitButton, false);

            // Mostrar botones de logueado
            SetDisplay(continueButton, true);
            SetDisplay(newGameButton, true);
            SetDisplay(logoutButton, true);

            // Ocultar login si esta visible
            HideLogin();
        }

        private void ShowLoggedOutState()
        {
            // Ocultar mensaje de bienvenida
            if (welcomeLabel != null)
                welcomeLabel.style.display = DisplayStyle.None;

            // Mostrar botones de no logueado
            SetDisplay(loginButton, true);
            SetDisplay(exitButton, true);

            // Ocultar botones de logueado
            SetDisplay(continueButton, false);
            SetDisplay(newGameButton, false);
            SetDisplay(logoutButton, false);
        }

        private void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        #region Button Handlers

        private void OnLoginClicked()
        {
            ShowLogin();
        }

        private void OnExitClicked()
        {
            Debug.Log("[MainMenuController] Saliendo del juego...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        private void OnContinueClicked()
        {
            Debug.Log("[MainMenuController] Continuando partida...");

            // Cargar partida guardada
            if (GameManager.Instance != null)
                GameManager.Instance.LoadGame();

            // Cargar escena del juego
            LoadGameScene();
        }

        private void OnNewGameClicked()
        {
            Debug.Log("[MainMenuController] Nueva partida...");

            // Iniciar nueva partida
            if (GameManager.Instance != null)
                GameManager.Instance.NewGame();

            // Crear nueva partida en la API
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.CreateGame(
                    game => Debug.Log($"[MainMenuController] Partida creada: {game.game_id}"),
                    error => Debug.LogWarning($"[MainMenuController] Error creando partida: {error}")
                );
            }

            // Cargar escena del juego
            LoadGameScene();
        }

        private void OnLogoutClicked()
        {
            Debug.Log("[MainMenuController] Cerrando sesion...");

            if (TriskelAPIClient.Instance != null)
                TriskelAPIClient.Instance.Logout();
        }

        #endregion

        #region Login Panel

        private void ShowLogin()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.Flex;
        }

        private void HideLogin()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.None;
        }

        #endregion

        #region Event Handlers

        private void OnLoggedIn()
        {
            // Obtener perfil para mostrar nombre
            TriskelAPIClient.Instance?.GetMyProfile(
                profile => ShowLoggedInState(profile.username),
                error => ShowLoggedInState()
            );
        }

        private void OnLoggedOut()
        {
            ShowLoggedOutState();
        }

        #endregion

        private void LoadGameScene()
        {
            if (!string.IsNullOrEmpty(gameSceneName))
                SceneManager.LoadScene(gameSceneName);
            else
                Debug.LogWarning("[MainMenuController] Nombre de escena del juego no configurado");
        }
    }
}
