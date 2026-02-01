// =======================================================================================
// Triskel RPG 2D - Main Menu Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador del menú principal del juego. Coordina los overlays de login
//              y registro, gestiona el estado de sesión del jugador y muestra diferentes
//              opciones según si el jugador está autenticado o no. Se comunica con la
//              API REST de Triskel para verificar sesiones y gestionar partidas.
// =======================================================================================

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using Triskel.API;
using Triskel.API.Models;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del menú principal del juego.
    /// </summary>
    /// <remarks>
    /// Este controlador orquesta el menú principal, mostrando diferentes opciones según
    /// el estado de autenticación del jugador:
    /// - Sin sesión: Botones de "Login" y "Salir"
    /// - Con sesión: Botones de "Continuar" (si hay partida activa), "Nueva Partida" y "Cerrar Sesión"
    ///
    /// Coordina los overlays de LoginController y RegisterController, y se suscribe a eventos
    /// del TriskelAPIClient para reaccionar a cambios de sesión.
    /// </remarks>
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

        /// <summary>
        /// Inicializa la UI del menú principal obteniendo referencias a elementos y registrando eventos.
        /// </summary>
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

        /// <summary>
        /// Verifica el estado de sesión del jugador al iniciar el menú.
        /// </summary>
        /// <remarks>
        /// Si hay una sesión guardada, verifica su validez con la API.
        /// Muestra el estado apropiado según el resultado.
        /// </remarks>
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

        /// <summary>
        /// Muestra el menú en estado de sesión iniciada (jugador autenticado).
        /// </summary>
        /// <param name="username">Nombre del jugador a mostrar en el mensaje de bienvenida (opcional).</param>
        /// <remarks>
        /// Muestra botones de "Continuar" (si hay partida activa), "Nueva Partida" y "Cerrar Sesión".
        /// Oculta botones de "Login" y "Salir".
        /// </remarks>
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

        /// <summary>
        /// Muestra el menú en estado sin sesión (jugador no autenticado).
        /// </summary>
        /// <remarks>
        /// Muestra botones de "Login" y "Salir".
        /// Oculta botones de sesión activa.
        /// </remarks>
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

        /// <summary>
        /// Método auxiliar para mostrar u ocultar un elemento de UI.
        /// </summary>
        /// <param name="element">Elemento de UI a modificar.</param>
        /// <param name="visible">True para mostrar, False para ocultar.</param>
        private void SetDisplay(VisualElement element, bool visible)
        {
            if (element != null)
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// Muestra un indicador de carga y oculta todos los botones del menú.
        /// </summary>
        /// <param name="message">Mensaje a mostrar en el indicador de carga.</param>
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

        /// <summary>
        /// Oculta el indicador de carga.
        /// </summary>
        private void HideLoading()
        {
            SetDisplay(loadingIndicator, false);
        }

        #region Button Handlers

        /// <summary>
        /// Callback cuando se hace clic en el botón "Login".
        /// Muestra el overlay de login.
        /// </summary>
        private void OnLoginClicked()
        {
            ShowLogin();
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Salir".
        /// Cierra la aplicación.
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("[MainMenuController] Saliendo del juego...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Continuar".
        /// Carga la partida guardada desde la API y continúa desde donde se quedó el jugador.
        /// </summary>
        private void OnContinueClicked()
        {
            Debug.Log("[MainMenuController] Continuando partida...");

            // Prevenir doble-click
            if (continueButton != null)
                continueButton.SetEnabled(false);

            if (TriskelAPIClient.Instance == null || string.IsNullOrEmpty(TriskelAPIClient.Instance.CurrentGameID))
            {
                Debug.LogWarning("[MainMenuController] No hay partida activa en la API");
                // Fallback a cargar local
                if (GameManager.Instance != null)
                    GameManager.Instance.LoadGame();
                LoadGameScene();
                return;
            }

            // Mostrar loading
            ShowLoading("Cargando partida...");

            // Obtener datos de la partida desde la API
            TriskelAPIClient.Instance.GetCurrentGame(
                game =>
                {
                    Debug.Log($"[MainMenuController] Partida cargada: {game.game_id}, Nivel: {game.current_level}");

                    // Restaurar estado del juego desde la API
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.RestoreFromAPI(game);
                    }

                    // Iniciar sesión de juego
                    TriskelAPIClient.Instance.StartSession(
                        session => Debug.Log($"[MainMenuController] Sesión iniciada: {session.session_id}"),
                        error => Debug.LogWarning($"[MainMenuController] Error iniciando sesión: {error}")
                    );

                    // IMPORTANTE: Iniciar tracking del nivel ANTES de cargar la escena
                    if (GameManager.Instance != null && !string.IsNullOrEmpty(game.current_level))
                    {
                        GameManager.Instance.GetAPITracker()?.OnLevelStart(game.current_level);
                        Debug.Log($"[MainMenuController] Tracking de nivel iniciado: {game.current_level}");
                    }

                    // Cargar la escena correspondiente al nivel actual
                    LoadLevelScene(game.current_level);
                },
                error =>
                {
                    Debug.LogError($"[MainMenuController] Error cargando partida: {error}");
                    HideLoading();
                    // Re-habilitar botón si falla
                    if (continueButton != null)
                        continueButton.SetEnabled(true);
                }
            );
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Nueva Partida".
        /// Crea una nueva partida en la API, inicia una sesión de juego y carga la escena del juego.
        /// </summary>
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

        /// <summary>
        /// Callback cuando se hace clic en el botón "Cerrar Sesión".
        /// Cierra la sesión del jugador mediante la API.
        /// </summary>
        private void OnLogoutClicked()
        {
            Debug.Log("[MainMenuController] Cerrando sesion...");

            if (TriskelAPIClient.Instance != null)
                TriskelAPIClient.Instance.Logout();
        }

        #endregion

        #region Auth Overlay Management

        /// <summary>
        /// Muestra el overlay de login y oculta el de registro.
        /// </summary>
        private void ShowLogin()
        {
            HideRegister();
            if (loginController != null)
                loginController.Show();
        }

        /// <summary>
        /// Muestra el overlay de registro y oculta el de login.
        /// </summary>
        private void ShowRegister()
        {
            HideLogin();
            if (registerController != null)
                registerController.Show();
        }

        /// <summary>
        /// Oculta el overlay de login.
        /// </summary>
        private void HideLogin()
        {
            if (loginController != null)
                loginController.Hide();
        }

        /// <summary>
        /// Oculta el overlay de registro.
        /// </summary>
        private void HideRegister()
        {
            if (registerController != null)
                registerController.Hide();
        }

        /// <summary>
        /// Oculta ambos overlays de autenticación (login y registro).
        /// </summary>
        private void HideAuthOverlays()
        {
            HideLogin();
            HideRegister();
        }

        /// <summary>
        /// Callback cuando la autenticación (login o registro) es exitosa.
        /// Oculta overlays, muestra loader y obtiene el perfil del jugador.
        /// </summary>
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

        /// <summary>
        /// Callback cuando el evento OnLoggedIn del TriskelAPIClient se dispara.
        /// Actualiza la UI al estado de sesión iniciada.
        /// </summary>
        private void OnLoggedIn()
        {
            ShowLoading("Cargando perfil...");

            // Obtener perfil para mostrar nombre
            TriskelAPIClient.Instance?.GetMyProfile(
                profile => ShowLoggedInState(profile.username),
                error => ShowLoggedInState()
            );
        }

        /// <summary>
        /// Callback cuando el evento OnLoggedOut del TriskelAPIClient se dispara.
        /// Actualiza la UI al estado sin sesión.
        /// </summary>
        private void OnLoggedOut()
        {
            ShowLoggedOutState();
        }

        #endregion

        /// <summary>
        /// Verifica si una escena está incluida en Build Settings.
        /// </summary>
        /// <param name="sceneName">Nombre de la escena a verificar.</param>
        /// <returns>True si la escena está en Build Settings, false en caso contrario.</returns>
        private bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Carga la escena del juego configurada en el Inspector.
        /// </summary>
        private void LoadGameScene()
        {
            if (!string.IsNullOrEmpty(gameSceneName))
                SceneManager.LoadScene(gameSceneName);
            else
                Debug.LogWarning("[MainMenuController] Nombre de escena del juego no configurado");
        }

        /// <summary>
        /// Carga la escena correspondiente a un nivel específico desde la API.
        /// </summary>
        /// <param name="apiLevel">Nivel desde la API (ej: "senda_ebano", "hub_central").</param>
        /// <remarks>
        /// Usa SceneConstants para convertir el nivel de la API a nombre de escena Unity.
        /// Valida que la escena esté en Build Settings antes de cargarla.
        /// </remarks>
        private void LoadLevelScene(string apiLevel)
        {
            // Convertir nivel API a nombre de escena Unity
            string sceneName = SceneConstants.GetSceneForAPILevel(apiLevel);

            // Validar que la escena esté en Build Settings
            if (!IsSceneInBuildSettings(sceneName))
            {
                Debug.LogError($"[MainMenuController] La escena '{sceneName}' no está en Build Settings. No se puede cargar.");
                // Fallback al hub si la escena no está disponible
                sceneName = SceneConstants.HUB;
                if (!IsSceneInBuildSettings(sceneName))
                {
                    Debug.LogError($"[MainMenuController] La escena del hub '{sceneName}' tampoco está en Build Settings. No se puede continuar.");
                    return;
                }
            }

            Debug.Log($"[MainMenuController] Cargando escena: {sceneName} (nivel API: {apiLevel})");
            SceneManager.LoadScene(sceneName);
        }
    }
}
