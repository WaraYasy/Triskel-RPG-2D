using UnityEngine;
using UnityEngine.UIElements;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de Login.
    /// Conecta la UI con TriskelAPIClient.
    /// </summary>
    public class LoginController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;

        private TextField userInput;
        private Button loginButton;
        private Label errorLabel;
        private VisualElement loginOverlay;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[LoginController] UIDocument no asignado");
                return;
            }

            var root = uiDocument.rootVisualElement;

            // Obtener referencias a elementos UI
            loginOverlay = root.Q<VisualElement>("LoginOverlay");
            userInput = root.Q<TextField>("UserInput");
            loginButton = root.Q<Button>("LoginButton");
            errorLabel = root.Q<Label>("ErrorLabel");

            // Configurar eventos
            if (loginButton != null)
                loginButton.clicked += OnLoginClicked;

            // Permitir Enter para enviar
            if (userInput != null)
            {
                userInput.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        OnLoginClicked();
                });
            }

            // Ocultar error inicialmente
            HideError();

            // Verificar si ya hay sesion
            CheckExistingSession();
        }

        private void OnDisable()
        {
            if (loginButton != null)
                loginButton.clicked -= OnLoginClicked;
        }

        private void CheckExistingSession()
        {
            if (TriskelAPIClient.Instance == null)
                return;

            if (TriskelAPIClient.Instance.IsLoggedIn)
            {
                // Verificar que la sesion siga siendo valida
                TriskelAPIClient.Instance.VerifySession(
                    profile =>
                    {
                        Debug.Log($"[LoginController] Sesion existente valida: {profile.username}");
                        Hide();
                    },
                    error =>
                    {
                        Debug.Log("[LoginController] Sesion invalida, mostrando login");
                        Show();
                    }
                );
            }
            else
            {
                Show();
            }
        }

        private void OnLoginClicked()
        {
            if (userInput == null)
                return;

            string username = userInput.value?.Trim();

            // Validar entrada
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Por favor, ingresa un nombre de usuario");
                return;
            }

            if (username.Length < 3)
            {
                ShowError("El nombre debe tener al menos 3 caracteres");
                return;
            }

            // Deshabilitar boton mientras se procesa
            SetLoading(true);
            HideError();

            // Intentar registro/login
            TriskelAPIClient.Instance.RegisterPlayer(
                username,
                null,
                response =>
                {
                    Debug.Log($"[LoginController] Login exitoso: {response.username}");
                    SetLoading(false);
                    Hide();
                },
                error =>
                {
                    Debug.LogWarning($"[LoginController] Error: {error}");
                    SetLoading(false);
                    ShowError(ParseError(error));
                }
            );
        }

        private string ParseError(string error)
        {
            // Parsear errores comunes de la API
            if (error.Contains("409") || error.Contains("already exists"))
                return "Este nombre de usuario ya esta en uso";

            if (error.Contains("422") || error.Contains("validation"))
                return "Nombre de usuario invalido";

            if (error.Contains("timeout") || error.Contains("Timeout"))
                return "Error de conexion. Intenta de nuevo";

            if (error.Contains("Unable to connect") || error.Contains("No connection"))
                return "No se pudo conectar al servidor";

            return "Error al iniciar sesion. Intenta de nuevo";
        }

        private void ShowError(string message)
        {
            if (errorLabel == null)
                return;

            errorLabel.text = message;
            errorLabel.style.display = DisplayStyle.Flex;
        }

        private void HideError()
        {
            if (errorLabel == null)
                return;

            errorLabel.style.display = DisplayStyle.None;
        }

        private void SetLoading(bool loading)
        {
            if (loginButton != null)
            {
                loginButton.SetEnabled(!loading);
                loginButton.text = loading ? "..." : "Entrar";
            }
        }

        public void Show()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.None;
        }
    }
}
