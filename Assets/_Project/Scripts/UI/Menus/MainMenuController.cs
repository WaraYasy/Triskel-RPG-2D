using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del Menu Principal.
    /// Muestra diferentes opciones segun el estado de sesion.
    /// Coordina los overlays de Login y Registro.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private LoginController loginController;
        [SerializeField] private RegisterController registerController;

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
        private VisualElement loadingIndicator;
        private Label loadingLabel;

        private void OnEnable()
        {
            InitializeUI();
        }

        private void Start()
        {
            // Suscribirse a eventos del API
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.OnLoggedIn += OnLoggedIn;
                TriskelAPIClient.Instance.OnLoggedOut += OnLoggedOut;
            }

            // Suscribirse a eventos de los controladores de auth
            if (loginController != null)
            {
                loginController.OnGoToRegister += ShowRegister;
                loginController.OnLoginSuccess += OnAuthSuccess;
            }

            if (registerController != null)
            {
                registerController.OnGoToLogin += ShowLogin;
                registerController.OnRegisterSuccess += OnAuthSuccess;
            }

            // Verificar estado inicial
            CheckSessionState();
        }

        private void OnDisable()
        {
            // Desuscribirse de eventos del API
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.OnLoggedIn -= OnLoggedIn;
                TriskelAPIClient.Instance.OnLoggedOut -= OnLoggedOut;
            }

            // Desuscribirse de eventos de auth
            if (loginController != null)
            {
                loginController.OnGoToRegister -= ShowRegister;
                loginController.OnLoginSuccess -= OnAuthSuccess;
            }

            if (registerController != null)
            {
                registerController.OnGoToLogin -= ShowLogin;
                registerController.OnRegisterSuccess -= OnAuthSuccess;
            }

            // Limpiar eventos de botones
            if (loginButton != null) loginButton.clicked -= OnLoginClicked;
            if (exitButton != null) exitButton.clicked -= OnExitClicked;
            if (continueButton != null) continueButton.clicked -= OnContinueClicked;
            if (newGameButton != null) newGameButton.clicked -= OnNewGameClicked;
            if (logoutButton != null) logoutButton.clicked -= OnLogoutClicked;
        }

        private void InitializeUI()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[MainMenuController] UIDocument no asignado");
                return;
            }

            var root = uiDocument.rootVisualElement;

            // Obtener referencias
            mainMenuOverlay = root.Q<VisualElement>("MainMenuOverlay");
            welcomeLabel = root.Q<Label>("WelcomeLabel");
            loginButton = root.Q<Button>("LoginButton");
            exitButton = root.Q<Button>("ExitButton");
            continueButton = root.Q<Button>("ContinueButton");
            newGameButton = root.Q<Button>("NewGameButton");
            logoutButton = root.Q<Button>("LogoutButton");
            loadingIndicator = root.Q<VisualElement>("LoadingIndicator");
            loadingLabel = root.Q<Label>("LoadingLabel");

            // Configurar eventos
            if (loginButton != null) loginButton.clicked += OnLoginClicked;
            if (exitButton != null) exitButton.clicked += OnExitClicked;
            if (continueButton != null) continueButton.clicked += OnContinueClicked;
            if (newGameButton != null) newGameButton.clicked += OnNewGameClicked;
            if (logoutButton != null) logoutButton.clicked += OnLogoutClicked;
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
                // Mostrar loader mientras verifica
                ShowLoading("Verificando sesion...");

                // Verificar que la sesion sea valida
                TriskelAPIClient.Instance.VerifySession(
                    profile =>
                    {
                        HideLoading();
                        ShowLoggedInState(profile.username);
                    },
                    error =>
                    {
                        HideLoading();
                        ShowLoggedOutState();
                    }
                );
            }
            else
            {
                ShowLoggedOutState();
            }
        }

        private void ShowLoggedInState(string username = null)
        {
            HideLoading();

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

            // Verificar si hay partida activa para mostrar "Continuar"
            bool hasActiveGame = TriskelAPIClient.Instance != null &&
                                 !string.IsNullOrEmpty(TriskelAPIClient.Instance.CurrentGameID);

            // Mostrar botones de logueado
            SetDisplay(continueButton, hasActiveGame); // Solo si hay partida activa
            SetDisplay(newGameButton, true);
            SetDisplay(logoutButton, true);

            // Ocultar overlays de auth
            HideAuthOverlays();
        }

        private void ShowLoggedOutState()
        {
            HideLoading();

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

            // Asegurar que los overlays esten ocultos
            HideAuthOverlays();
        }

        private void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ShowLoading(string message = "Cargando...")
        {
            if (loadingLabel != null)
                loadingLabel.text = message;

            SetDisplay(loadingIndicator, true);

            // Ocultar todos los botones mientras carga
            SetDisplay(loginButton, false);
            SetDisplay(exitButton, false);
            SetDisplay(continueButton, false);
            SetDisplay(newGameButton, false);
            SetDisplay(logoutButton, false);
            SetDisplay(welcomeLabel, false);
        }

        private void HideLoading()
        {
            SetDisplay(loadingIndicator, false);
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

            // Prevenir doble-click
            if (continueButton != null)
                continueButton.SetEnabled(false);

            // Cargar partida guardada
            if (GameManager.Instance != null)
                GameManager.Instance.LoadGame();

            // Cargar escena del juego
            LoadGameScene();
        }

        private void OnNewGameClicked()
        {
            Debug.Log("[MainMenuController] Nueva partida...");

            // Deshabilitar botón para prevenir doble-click
            if (newGameButton != null)
                newGameButton.SetEnabled(false);

            // Iniciar nueva partida
            if (GameManager.Instance != null)
                GameManager.Instance.NewGame();

            // Crear nueva partida en la API
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.CreateGame(
                    game =>
                    {
                        Debug.Log($"[MainMenuController] Partida creada: {game.game_id}");
                        // Iniciar sesion de juego
                        TriskelAPIClient.Instance.StartSession(
                            session => Debug.Log($"[MainMenuController] Sesion iniciada: {session.session_id}"),
                            error => Debug.LogWarning($"[MainMenuController] Error iniciando sesion: {error}")
                        );

                        // Cargar escena del juego
                        LoadGameScene();
                    },
                    error =>
                    {
                        Debug.LogWarning($"[MainMenuController] Error creando partida: {error}");
                        // Re-habilitar botón si falla
                        if (newGameButton != null)
                            newGameButton.SetEnabled(true);
                    }
                );
            }
            else
            {
                // Si no hay API, cargar escena directamente
                LoadGameScene();
            }
        }

        private void OnLogoutClicked()
        {
            Debug.Log("[MainMenuController] Cerrando sesion...");

            if (TriskelAPIClient.Instance != null)
                TriskelAPIClient.Instance.Logout();
        }

        #endregion

        #region Auth Overlay Management

        private void ShowLogin()
        {
            HideRegister();
            if (loginController != null)
                loginController.Show();
        }

        private void ShowRegister()
        {
            HideLogin();
            if (registerController != null)
                registerController.Show();
        }

        private void HideLogin()
        {
            if (loginController != null)
                loginController.Hide();
        }

        private void HideRegister()
        {
            if (registerController != null)
                registerController.Hide();
        }

        private void HideAuthOverlays()
        {
            HideLogin();
            HideRegister();
        }

        private void OnAuthSuccess()
        {
            Debug.Log("[MainMenuController] Autenticacion exitosa");

            // Ocultar overlays
            HideAuthOverlays();

            // Mostrar loader mientras obtiene perfil
            ShowLoading("Cargando perfil...");

            // Obtener perfil y actualizar UI
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.GetMyProfile(
                    profile => ShowLoggedInState(profile.username),
                    error => ShowLoggedInState()
                );
            }
            else
            {
                HideLoading();
            }
        }

        #endregion

        #region Event Handlers

        private void OnLoggedIn()
        {
            ShowLoading("Cargando perfil...");

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
