// =======================================================================================
// Triskel RPG 2D - Gameplay UI Manager
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Gestor de UI durante el gameplay. Maneja alertas de error de conexión
//              y otros elementos de UI que deben mostrarse durante las partidas.
//              Se suscribe a eventos de la API para mostrar alertas cuando sea necesario.
// =======================================================================================

using UnityEngine;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Gestor central de UI durante el gameplay.
    /// </summary>
    /// <remarks>
    /// SINGLETON: Persiste entre escenas con DontDestroyOnLoad.
    /// Se suscribe a eventos de TriskelAPIClient para mostrar alertas de error
    /// de conexión cuando falla la comunicación con el servidor.
    ///
    /// IMPORTANTE: Requiere que exista un ConnectionErrorAlertController.Instance.
    /// </remarks>
    public class GameplayUIManager : MonoBehaviour
    {
        public static GameplayUIManager Instance { get; private set; }

        [Header("Referencias")]
        [SerializeField] private ConnectionErrorAlertController connectionErrorAlert;

        [Header("Configuración")]
        [Tooltip("Mostrar alerta solo la primera vez que falla la conexión (evita spam)")]
        [SerializeField] private bool showOnlyOnce = true;

        private bool hasShownConnectionError = false;

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
            // Usar la instancia Singleton de ConnectionErrorAlertController
            if (connectionErrorAlert == null)
            {
                connectionErrorAlert = ConnectionErrorAlertController.Instance;
            }

            if (connectionErrorAlert == null)
            {
                Debug.LogWarning("[GameplayUIManager] ConnectionErrorAlertController no encontrado en la escena");
            }
        }

        private void Start()
        {
            // Suscribirse en Start() para asegurar que TriskelAPIClient.Awake() ya se ejecutó
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.OnConnectionError += HandleConnectionError;
                Debug.Log("[GameplayUIManager] Suscrito a eventos de conexión de la API");
            }
            else
            {
                Debug.LogError("[GameplayUIManager] TriskelAPIClient.Instance no encontrado");
            }
        }

        private void OnDisable()
        {
            // Desuscribirse de eventos
            if (TriskelAPIClient.Instance != null)
            {
                TriskelAPIClient.Instance.OnConnectionError -= HandleConnectionError;
            }
        }

        /// <summary>
        /// Maneja los errores de conexión mostrando la alerta al jugador.
        /// </summary>
        private void HandleConnectionError()
        {
            // Si ya mostramos la alerta y está configurado para mostrar solo una vez, salir
            if (showOnlyOnce && hasShownConnectionError)
            {
                Debug.LogWarning("[GameplayUIManager] Error de conexión detectado (alerta ya mostrada anteriormente)");
                return;
            }

            Debug.LogWarning("[GameplayUIManager] Error de conexión detectado - Mostrando alerta");

            if (connectionErrorAlert != null)
            {
                // Mostrar alerta con callback de reintentar
                connectionErrorAlert.Show(
                    customMessage: null, // Usar mensaje por defecto
                    onRetry: AttemptReconnect
                );

                hasShownConnectionError = true;
            }
            else
            {
                // FALLBACK: Si no hay alerta disponible, mostrar en consola
                Debug.LogError("===== ⚠️ ERROR DE CONEXIÓN =====");
                Debug.LogError("No se puede conectar al servidor.");
                Debug.LogError("El progreso NO se guardará.");
                Debug.LogError("================================");
            }
        }

        /// <summary>
        /// Intenta reconectar con el servidor verificando la sesión.
        /// </summary>
        /// <param name="onSuccess">Callback a ejecutar si la reconexión es exitosa (cierra la alerta).</param>
        private void AttemptReconnect(System.Action onSuccess)
        {
            Debug.Log("[GameplayUIManager] Intentando reconectar...");

            if (TriskelAPIClient.Instance == null)
            {
                Debug.LogError("[GameplayUIManager] TriskelAPIClient no disponible");
                connectionErrorAlert?.OnRetryFailed("Error interno al reconectar.");
                return;
            }

            // Verificar si hay sesión activa
            if (!TriskelAPIClient.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[GameplayUIManager] No hay sesión activa para reconectar");
                connectionErrorAlert?.OnRetryFailed("No hay sesión activa.");
                return;
            }

            // Intentar verificar sesión para comprobar conectividad
            TriskelAPIClient.Instance.VerifySession(
                profile =>
                {
                    Debug.Log($"[GameplayUIManager] ✓ Reconexión exitosa: {profile.username}");
                    hasShownConnectionError = false; // Resetear flag para permitir mostrar alerta de nuevo si falla
                    onSuccess?.Invoke(); // Cerrar alerta
                },
                error =>
                {
                    Debug.LogWarning($"[GameplayUIManager] ✗ Reconexión fallida: {error}");
                    connectionErrorAlert?.OnRetryFailed("No se pudo conectar al servidor.\n\nVerifica tu conexión a internet.");
                }
            );
        }

        /// <summary>
        /// Resetea el flag de alerta mostrada (útil al cambiar de escena o reiniciar nivel).
        /// </summary>
        public void ResetErrorState()
        {
            hasShownConnectionError = false;
            Debug.Log("[GameplayUIManager] Estado de error reseteado");
        }
    }
}
