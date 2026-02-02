// =======================================================================================
// Triskel RPG 2D - Connection Error Alert Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador de la alerta de error de conexión. Se muestra cuando hay
//              problemas de red al comunicarse con la API REST. Proporciona opciones
//              para reintentar la conexión o volver al menú principal.
// =======================================================================================

using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la alerta de error de conexión.
    /// </summary>
    /// <remarks>
    /// Esta alerta se muestra cuando hay fallos de conexión con la API durante
    /// operaciones críticas (guardado, login, etc.). Advierte al jugador que
    /// el progreso no se guardará y ofrece opciones para reintentar o salir.
    ///
    /// SINGLETON: Persiste entre escenas con DontDestroyOnLoad.
    /// Eventos disponibles: OnRetry, OnExit
    ///
    /// COMPORTAMIENTO:
    /// - Pausa TODO: Time.timeScale, Input, Diálogos
    /// - Aparece encima de TODO con sortingOrder 99999
    /// - El reintento mantiene la alerta abierta hasta que la conexión funcione
    /// </remarks>
    public class ConnectionErrorAlertController : MonoBehaviour
    {
        // Constantes
        private const int MAX_SORTING_ORDER = 99999; // Máxima prioridad - debe aparecer encima de TODO

        public static ConnectionErrorAlertController Instance { get; private set; }

        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Configuración")]
        [SerializeField] private string mainMenuSceneName = "Home";

        private VisualElement errorOverlay;
        private Button retryButton;
        private Button exitButton;
        private Label errorMessage;
        private string defaultMessage = "No tienes conexión con el servidor.\n\nNada de lo que hagas se guardará hasta que se restablezca la conexión.";

        // Callback para reintentar la operación fallida
        private Action<Action> retryActionWithCallback; // Recibe un callback de éxito

        // Estado antes de pausar
        private bool wasInputEnabled = true;

        // Referencias cacheadas para optimizar PauseEverything()
        private PlayerInput cachedPlayerInput;
        private Yarn.Unity.DialogueRunner cachedDialogueRunner;
        private PauseController cachedPauseController;
        private bool cacheInitialized = false;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            // Marcar el root GameObject como persistente (DontDestroyOnLoad solo funciona con root)
            DontDestroyOnLoad(transform.root.gameObject);
        }

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[ConnectionErrorAlert] UIDocument no asignado");
                return;
            }

            // Asegurar que aparezca encima de TODO
            // Usamos el valor MÁS ALTO posible para garantizar que esté encima
            if (uiDocument.panelSettings != null)
            {
                uiDocument.panelSettings.sortingOrder = MAX_SORTING_ORDER;
            }

            var root = uiDocument.rootVisualElement;

            // Obtener referencias a elementos UI
            errorOverlay = root.Q<VisualElement>("ErrorOverlay");
            retryButton = root.Q<Button>("RetryButton");
            exitButton = root.Q<Button>("ExitButton");
            errorMessage = root.Q<Label>("ErrorMessage");

            // Debug: Verificar que se encontraron los elementos
            Debug.Log($"[ConnectionErrorAlert] Elementos UI encontrados - ErrorOverlay: {errorOverlay != null}, RetryButton: {retryButton != null}, ExitButton: {exitButton != null}");

            // Configurar eventos
            if (retryButton != null)
            {
                retryButton.clicked += OnRetryClicked;
                Debug.Log("[ConnectionErrorAlert] ✓ Evento OnRetryClicked suscrito");
            }
            else
            {
                Debug.LogError("[ConnectionErrorAlert] ⚠️ RetryButton no encontrado - no se puede suscribir el evento");
            }

            if (exitButton != null)
                exitButton.clicked += OnExitClicked;

            // Ocultar por defecto
            Hide();
        }

        private void OnDisable()
        {
            if (retryButton != null)
                retryButton.clicked -= OnRetryClicked;

            if (exitButton != null)
                exitButton.clicked -= OnExitClicked;
        }

        /// <summary>
        /// Muestra la alerta de error de conexión con un mensaje personalizado.
        /// </summary>
        /// <param name="customMessage">Mensaje personalizado opcional. Si es null, usa el mensaje por defecto.</param>
        /// <param name="onRetry">Callback a ejecutar cuando el usuario haga clic en "Reintentar".
        /// El callback recibe un Action que DEBE llamar si la reconexión es exitosa para cerrar la alerta.</param>
        /// <remarks>
        /// Pausa TODO: Time.timeScale, Input System, Diálogos de Yarn Spinner.
        /// La alerta aparece con sortingOrder 99999 para estar encima de TODO.
        /// El callback de retry recibe un Action que debe invocar SI Y SOLO SI la conexión funciona.
        /// </remarks>
        public void Show(string customMessage = null, Action<Action> onRetry = null)
        {
            if (errorOverlay == null)
            {
                Debug.LogError("[ConnectionErrorAlert] ErrorOverlay no encontrado");
                return;
            }

            // Guardar callback de reintentar
            retryActionWithCallback = onRetry;

            if (onRetry == null)
            {
                Debug.LogWarning("[ConnectionErrorAlert] ⚠️ No se proporcionó callback de reintento");
            }
            else
            {
                Debug.Log("[ConnectionErrorAlert] ✓ Callback de reintento configurado");
            }

            // Actualizar mensaje
            if (errorMessage != null)
            {
                errorMessage.text = !string.IsNullOrEmpty(customMessage) ? customMessage : defaultMessage;
            }

            // Mostrar overlay
            errorOverlay.style.display = DisplayStyle.Flex;

            // PAUSAR TODO EL JUEGO
            PauseEverything();

            Debug.LogWarning("[ConnectionErrorAlert] Alerta mostrada - TODO pausado");
        }

        /// <summary>
        /// Oculta la alerta de error de conexión.
        /// </summary>
        /// <remarks>
        /// Restaura el Time.timeScale a 1 y reactiva el input.
        /// </remarks>
        public void Hide()
        {
            if (errorOverlay != null)
                errorOverlay.style.display = DisplayStyle.None;

            // Restaurar mensaje por defecto
            if (errorMessage != null)
            {
                errorMessage.text = defaultMessage;
            }

            // Limpiar callback
            retryActionWithCallback = null;

            // REANUDAR TODO
            ResumeEverything();
        }

        /// <summary>
        /// Inicializa el cache de referencias para evitar FindFirstObjectByType repetidos.
        /// Se llama lazy la primera vez que se pausa.
        /// </summary>
        private void InitializeCache()
        {
            if (cacheInitialized) return;

            cachedPlayerInput = FindFirstObjectByType<PlayerInput>();
            cachedDialogueRunner = FindFirstObjectByType<Yarn.Unity.DialogueRunner>();
            cachedPauseController = FindFirstObjectByType<PauseController>();

            cacheInitialized = true;

            Debug.Log($"[ConnectionErrorAlert] Cache inicializado (PlayerInput: {cachedPlayerInput != null}, DialogueRunner: {cachedDialogueRunner != null}, PauseController: {cachedPauseController != null})");
        }

        /// <summary>
        /// Pausa TODO: Time.timeScale, Input, Diálogos, otros menús.
        /// </summary>
        private void PauseEverything()
        {
            // Inicializar cache si es la primera vez
            InitializeCache();

            // 1. Pausar tiempo del juego
            Time.timeScale = 0f;

            // 2. Desactivar Input del jugador
            if (cachedPlayerInput != null)
            {
                wasInputEnabled = cachedPlayerInput.enabled;
                cachedPlayerInput.enabled = false;
            }

            // 3. Pausar diálogos de Yarn Spinner (si está activo)
            // Yarn Spinner se pausa automáticamente con Time.timeScale = 0

            // 4. Ocultar menú de pausa si está abierto (para evitar conflictos)
            if (cachedPauseController != null && cachedPauseController.IsPaused)
            {
                cachedPauseController.Hide(); // Solo ocultar UI, no resumir el juego
            }
        }

        /// <summary>
        /// Reanuda TODO: Time.timeScale, Input.
        /// </summary>
        private void ResumeEverything()
        {
            // 1. Reanudar tiempo
            Time.timeScale = 1f;

            // 2. Reactivar Input del jugador (usa cache)
            if (cachedPlayerInput != null && wasInputEnabled)
            {
                cachedPlayerInput.enabled = true;
            }
        }

        /// <summary>
        /// Callback cuando se hace clic en "Reintentar".
        /// IMPORTANTE: NO cierra la alerta inmediatamente.
        /// Solo se cierra si el callback de retry tiene éxito.
        /// </summary>
        private void OnRetryClicked()
        {
            Debug.Log("[ConnectionErrorAlert] Usuario hizo clic en Reintentar...");

            // Verificar si el callback existe
            if (retryActionWithCallback == null)
            {
                Debug.LogError("[ConnectionErrorAlert] ⚠️ No hay callback de reintento configurado!");
                if (errorMessage != null)
                {
                    errorMessage.text = "Error interno: No se puede reintentar.\n\nIntenta salir al menú.";
                }
                return;
            }

            // Cambiar mensaje a "Reintentando..."
            if (errorMessage != null)
            {
                errorMessage.text = "Reintentando conexión...\n\nEspera un momento.";
            }

            // Deshabilitar botones mientras se reintenta
            if (retryButton != null) retryButton.SetEnabled(false);
            if (exitButton != null) exitButton.SetEnabled(false);

            Debug.Log("[ConnectionErrorAlert] Ejecutando callback de reintento...");

            // Ejecutar callback de reintento
            // Le pasamos un callback que DEBE invocar SI la conexión funciona
            retryActionWithCallback?.Invoke(OnRetrySuccess);
        }

        /// <summary>
        /// Callback de éxito del reintento.
        /// Se llama SOLO si la reconexión fue exitosa.
        /// </summary>
        private void OnRetrySuccess()
        {
            Debug.Log("[ConnectionErrorAlert] ✓ OnRetrySuccess - Cerrando alerta...");
            Hide();
            Debug.Log("[ConnectionErrorAlert] ✓ Alerta cerrada exitosamente");
        }

        /// <summary>
        /// Callback cuando falla el reintento.
        /// Restaura los botones y el mensaje de error.
        /// </summary>
        public void OnRetryFailed(string errorMsg = null)
        {

            // Restaurar mensaje de error
            if (errorMessage != null)
            {
                string msg = !string.IsNullOrEmpty(errorMsg) ? errorMsg : defaultMessage;
                errorMessage.text = msg + "\n\n(Intenta de nuevo o sal al menú)";
            }

            // Rehabilitar botones
            if (retryButton != null) retryButton.SetEnabled(true);
            if (exitButton != null) exitButton.SetEnabled(true);
        }

        /// <summary>
        /// Callback cuando se hace clic en "Salir al Menú".
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("[ConnectionErrorAlert] Saliendo al menú principal...");

            // Guardar partida localmente antes de salir (sin API)
            if (GameManager.Instance != null)
            {
                try
                {
                    GameManager.Instance.SaveGame();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ConnectionErrorAlert] No se pudo guardar localmente: {ex.Message}");
                }
            }

            // Ocultar alerta
            Hide();

            // Cargar menú principal
            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Verifica si la alerta está actualmente visible.
        /// </summary>
        public bool IsVisible => errorOverlay != null && errorOverlay.style.display == DisplayStyle.Flex;
    }
}
