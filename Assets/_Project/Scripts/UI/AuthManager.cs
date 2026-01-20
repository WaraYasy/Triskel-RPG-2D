using UnityEngine;
using UnityEngine.SceneManagement;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Gestor de autenticacion que coordina las pantallas de Login y Registro.
    /// Maneja la navegacion entre ellas y la transicion al juego tras login exitoso.
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private LoginController loginController;
        [SerializeField] private RegisterController registerController;

        [Header("Configuracion de Escenas")]
        [SerializeField] private string hubSceneName = "Hub";

        private void OnEnable()
        {
            // Verificar referencias
            if (loginController == null)
            {
                Debug.LogError("[AuthManager] LoginController no asignado");
                return;
            }

            if (registerController == null)
            {
                Debug.LogError("[AuthManager] RegisterController no asignado");
                return;
            }

            // Suscribirse a eventos de navegacion
            loginController.OnGoToRegister += ShowRegister;
            loginController.OnLoginSuccess += OnAuthSuccess;

            registerController.OnGoToLogin += ShowLogin;
            registerController.OnRegisterSuccess += OnAuthSuccess;

            // Mostrar login al inicio
            ShowLogin();
        }

        private void OnDisable()
        {
            // Desuscribirse de eventos
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
        }

        /// <summary>
        /// Muestra la pantalla de login y oculta la de registro.
        /// </summary>
        private void ShowLogin()
        {
            Debug.Log("[AuthManager] Mostrando Login");
            registerController.Hide();
            loginController.Show();
        }

        /// <summary>
        /// Muestra la pantalla de registro y oculta la de login.
        /// </summary>
        private void ShowRegister()
        {
            Debug.Log("[AuthManager] Mostrando Registro");
            loginController.Hide();
            registerController.Show();
        }

        /// <summary>
        /// Llamado cuando el login o registro es exitoso.
        /// Carga la informacion del jugador y lo envia al Hub o nivel correspondiente.
        /// </summary>
        private void OnAuthSuccess()
        {
            Debug.Log("[AuthManager] Autenticacion exitosa");

            // Obtener perfil del jugador para verificar si hay partida activa
            TriskelAPIClient.Instance.GetMyProfile(
                profile =>
                {
                    Debug.Log($"[AuthManager] Perfil cargado: {profile.username}");

                    // Verificar si hay partida activa
                    if (!string.IsNullOrEmpty(TriskelAPIClient.Instance.CurrentGameID))
                    {
                        Debug.Log($"[AuthManager] Partida activa encontrada: {TriskelAPIClient.Instance.CurrentGameID}");
                        // Iniciar sesion de juego
                        StartGameSession();
                    }
                    else
                    {
                        Debug.Log("[AuthManager] No hay partida activa, ir al Hub");
                        LoadHub();
                    }
                },
                error =>
                {
                    Debug.LogWarning($"[AuthManager] Error al cargar perfil: {error}");
                    // Si falla, ir al Hub de todas formas
                    LoadHub();
                }
            );
        }

        /// <summary>
        /// Inicia una sesion de juego en el servidor.
        /// </summary>
        private void StartGameSession()
        {
            TriskelAPIClient.Instance.CreateSession(
                session =>
                {
                    Debug.Log($"[AuthManager] Sesion de juego iniciada: {session.session_id}");
                    LoadHub();
                },
                error =>
                {
                    Debug.LogWarning($"[AuthManager] Error al iniciar sesion: {error}");
                    // Si falla, ir al Hub de todas formas
                    LoadHub();
                }
            );
        }

        /// <summary>
        /// Carga la escena del Hub.
        /// </summary>
        private void LoadHub()
        {
            Debug.Log($"[AuthManager] Cargando escena: {hubSceneName}");

            if (string.IsNullOrEmpty(hubSceneName))
            {
                Debug.LogError("[AuthManager] Nombre de escena del Hub no configurado");
                return;
            }

            SceneManager.LoadScene(hubSceneName);
        }
    }
}
