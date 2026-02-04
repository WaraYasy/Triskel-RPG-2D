// =======================================================================================
// Triskel RPG 2D - Login Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador de la pantalla de login (inicio de sesión). Funciona como
//              overlay dentro del menú principal y gestiona la autenticación de
//              jugadores mediante la API REST de Triskel.
// =======================================================================================

using System;
using UnityEngine;
using UnityEngine.UIElements;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de login del juego.
    /// </summary>
    /// <remarks>
    /// Este controlador gestiona el formulario de inicio de sesión, validando entradas
    /// y comunicándose con el TriskelAPIClient para autenticar al jugador.
    /// Funciona como un overlay que se muestra sobre el menú principal.
    ///
    /// Eventos disponibles: OnGoToRegister, OnLoginSuccess
    /// </remarks>
    public class LoginController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;

        private TextField userInput;
        private TextField passwordInput;
        private Button loginButton;
        private Button goToRegisterButton;
        private Label errorLabel;
        private VisualElement loginOverlay;

        // Eventos para comunicarse con MainMenuController
        /// <summary>
        /// Evento que se dispara cuando el jugador hace clic en "Ir a Registro".
        /// </summary>
        public event Action OnGoToRegister;
        /// <summary>
        /// Evento que se dispara cuando el login es exitoso.
        /// </summary>
        public event Action OnLoginSuccess;

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
            passwordInput = root.Q<TextField>("PasswordInput");
            loginButton = root.Q<Button>("LoginButton");
            goToRegisterButton = root.Q<Button>("GoToRegisterButton");
            errorLabel = root.Q<Label>("ErrorLabel");

            // Configurar eventos
            if (loginButton != null)
                loginButton.clicked += OnLoginClicked;

            if (goToRegisterButton != null)
                goToRegisterButton.clicked += OnGoToRegisterClicked;

            // Permitir Enter para enviar
            if (passwordInput != null)
            {
                passwordInput.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        OnLoginClicked();
                });
            }

            // Ocultar por defecto
            Hide();
        }

        private void OnDisable()
        {
            if (loginButton != null)
                loginButton.clicked -= OnLoginClicked;

            if (goToRegisterButton != null)
                goToRegisterButton.clicked -= OnGoToRegisterClicked;
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Entrar".
        /// Valida las entradas y realiza la petición de login a la API.
        /// </summary>
        private void OnLoginClicked()
        {
            if (!ValidateInputs())
                return;

            string username = userInput.value?.Trim();
            string password = passwordInput.value;

            SetLoading(true);
            HideError();

            TriskelAPIClient.Instance.Login(
                username,
                password,
                response =>
                {
                    Debug.Log($"[LoginController] Login exitoso: {response.username}");
                    SetLoading(false);
                    ClearFields();
                    OnLoginSuccess?.Invoke();
                },
                error =>
                {
                    Debug.LogWarning($"[LoginController] Error login: {error}");
                    SetLoading(false);
                    ShowError(ParseError(error));
                }
            );
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Ir a Registro".
        /// Limpia los campos y dispara el evento OnGoToRegister.
        /// </summary>
        private void OnGoToRegisterClicked()
        {
            ClearFields();
            OnGoToRegister?.Invoke();
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de login.
        /// </summary>
        /// <returns>True si las entradas son válidas, False en caso contrario.</returns>
        /// <remarks>
        /// Valida que el nombre de usuario tenga al menos 3 caracteres y que
        /// la contraseña no esté vacía.
        /// </remarks>
        private bool ValidateInputs()
        {
            string username = userInput?.value?.Trim();
            string password = passwordInput?.value;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Por favor, ingresa un nombre de usuario");
                return false;
            }

            if (username.Length < 3)
            {
                ShowError("El nombre debe tener al menos 3 caracteres");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Por favor, ingresa una contraseña");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parsea mensajes de error de la API y los convierte en mensajes amigables para el usuario.
        /// </summary>
        /// <param name="error">Mensaje de error de la API.</param>
        /// <returns>Mensaje de error amigable en español.</returns>
        private string ParseError(string error)
        {
            if (error.Contains("401") || error.Contains("Unauthorized"))
                return "Usuario o contraseña incorrectos";

            if (error.Contains("timeout") || error.Contains("Timeout"))
                return "Error de conexion. Intenta de nuevo";

            if (error.Contains("Unable to connect") || error.Contains("No connection"))
                return "No se pudo conectar al servidor";

            return "Error al iniciar sesion. Intenta de nuevo";
        }

        /// <summary>
        /// Muestra un mensaje de error en la UI.
        /// </summary>
        /// <param name="message">Mensaje de error a mostrar.</param>
        private void ShowError(string message)
        {
            if (errorLabel == null)
                return;

            errorLabel.text = message;
            errorLabel.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Oculta el mensaje de error en la UI.
        /// </summary>
        private void HideError()
        {
            if (errorLabel == null)
                return;

            errorLabel.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// Establece el estado de carga de la UI, deshabilitando/habilitando controles.
        /// </summary>
        /// <param name="loading">True para mostrar estado de carga, False para estado normal.</param>
        private void SetLoading(bool loading)
        {
            if (loginButton != null)
            {
                loginButton.SetEnabled(!loading);
                loginButton.text = loading ? "..." : "Entrar";
            }

            if (goToRegisterButton != null)
                goToRegisterButton.SetEnabled(!loading);

            if (userInput != null)
                userInput.SetEnabled(!loading);

            if (passwordInput != null)
                passwordInput.SetEnabled(!loading);
        }

        /// <summary>
        /// Limpia todos los campos del formulario de login.
        /// </summary>
        private void ClearFields()
        {
            if (userInput != null)
                userInput.value = "";

            if (passwordInput != null)
                passwordInput.value = "";

            HideError();
        }

        /// <summary>
        /// Muestra el overlay de login y limpia los campos del formulario.
        /// </summary>
        public void Show()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.Flex;

            ClearFields();
        }

        /// <summary>
        /// Oculta el overlay de login y limpia los campos del formulario.
        /// </summary>
        public void Hide()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.None;

            ClearFields();
        }
    }
}
