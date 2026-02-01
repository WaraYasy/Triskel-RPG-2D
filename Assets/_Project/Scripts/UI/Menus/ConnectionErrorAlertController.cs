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
    /// </remarks>
    public class ConnectionErrorAlertController : MonoBehaviour
    {
        public static ConnectionErrorAlertController Instance { get; private set; }

        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;

        [Header("Configuración")]
        [SerializeField] private string mainMenuSceneName = "Home";

        private VisualElement errorOverlay;
        private Button retryButton;
        private Button exitButton;
        private Label errorMessage;

        // Callback para reintentar la operación fallida
        private Action retryAction;

        // Eventos
        /// <summary>
        /// Evento que se dispara cuando el usuario hace clic en "Reintentar".
        /// </summary>
        public event Action OnRetry;
        /// <summary>
        /// Evento que se dispara cuando el usuario hace clic en "Salir".
        /// </summary>
        public event Action OnExit;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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

            var root = uiDocument.rootVisualElement;

            // Obtener referencias a elementos UI
            errorOverlay = root.Q<VisualElement>("ErrorOverlay");
            retryButton = root.Q<Button>("RetryButton");
            exitButton = root.Q<Button>("ExitButton");
            errorMessage = root.Q<Label>("ErrorMessage");

            // Configurar eventos
            if (retryButton != null)
                retryButton.clicked += OnRetryClicked;

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
        /// <param name="onRetry">Callback opcional a ejecutar cuando el usuario haga clic en "Reintentar".</param>
        /// <remarks>
        /// Pausa el juego (Time.timeScale = 0) mientras la alerta está visible.
        /// </remarks>
        public void Show(string customMessage = null, Action onRetry = null)
        {
            if (errorOverlay == null)
            {
                Debug.LogError("[ConnectionErrorAlert] ErrorOverlay no encontrado");
                return;
            }

            // Guardar callback de reintentar
            retryAction = onRetry;

            // Actualizar mensaje si se proporciona
            if (!string.IsNullOrEmpty(customMessage) && errorMessage != null)
            {
                errorMessage.text = customMessage;
            }

            // Mostrar overlay
            errorOverlay.style.display = DisplayStyle.Flex;

            // Pausar el juego
            Time.timeScale = 0f;

            Debug.Log("[ConnectionErrorAlert] Alerta de error de conexión mostrada");
        }

        /// <summary>
        /// Oculta la alerta de error de conexión.
        /// </summary>
        /// <remarks>
        /// Restaura el Time.timeScale a 1 (reanuda el juego).
        /// </remarks>
        public void Hide()
        {
            if (errorOverlay != null)
                errorOverlay.style.display = DisplayStyle.None;

            // Restaurar mensaje por defecto
            if (errorMessage != null)
            {
                errorMessage.text = "No tienes conexión con el servidor.\n\nNada de lo que hagas se guardará hasta que se restablezca la conexión.";
            }

            // Limpiar callback
            retryAction = null;

            // Reanudar el juego
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Callback cuando se hace clic en "Reintentar".
        /// </summary>
        private void OnRetryClicked()
        {
            Debug.Log("[ConnectionErrorAlert] Reintentando conexión...");

            // Ocultar alerta
            Hide();

            // Ejecutar callback si existe
            retryAction?.Invoke();

            // Disparar evento
            OnRetry?.Invoke();
        }

        /// <summary>
        /// Callback cuando se hace clic en "Salir al Menú".
        /// </summary>
        private void OnExitClicked()
        {
            Debug.Log("[ConnectionErrorAlert] Saliendo al menú principal...");

            // Guardar partida antes de salir (si es posible)
            if (GameManager.Instance != null)
            {
                try
                {
                    GameManager.Instance.SaveGame();
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[ConnectionErrorAlert] No se pudo guardar: {ex.Message}");
                }
            }

            // Ocultar alerta
            Hide();

            // Disparar evento
            OnExit?.Invoke();

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
