// =======================================================================================
// Triskel RPG 2D - Register Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador de la pantalla de registro (creación de cuenta). Funciona
//              como overlay dentro del menú principal y gestiona el registro de nuevos
//              jugadores mediante la API REST de Triskel.
// =======================================================================================

using System;
using UnityEngine;
using UnityEngine.UIElements;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de registro del juego.
    /// </summary>
    /// <remarks>
    /// Este controlador gestiona el formulario de registro, validando entradas
    /// (nombre de usuario, contraseña, confirmación y email opcional) y comunicándose
    /// con el TriskelAPIClient para crear nuevas cuentas de jugador.
    /// Funciona como un overlay que se muestra sobre el menú principal.
    ///
    /// Eventos disponibles: OnGoToLogin, OnRegisterSuccess
    /// </remarks>
    public class RegisterController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;

        private TextField userInput;
        private TextField passwordInput;
        private TextField confirmPasswordInput;
        private TextField emailInput;
        private Button registerButton;
        private Button goToLoginButton;
        private Label errorLabel;
        private VisualElement registerOverlay;

        // Eventos para comunicarse con MainMenuController
        /// <summary>
        /// Evento que se dispara cuando el jugador hace clic en "Ir a Login".
        /// </summary>
        public event Action OnGoToLogin;
        /// <summary>
        /// Evento que se dispara cuando el registro es exitoso.
        /// </summary>
        public event Action OnRegisterSuccess;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[RegisterController] UIDocument no asignado");
                return;
            }

            var root = uiDocument.rootVisualElement;

            // Obtener referencias a elementos UI
            registerOverlay = root.Q<VisualElement>("RegisterOverlay");
            userInput = root.Q<TextField>("UserInput");
            passwordInput = root.Q<TextField>("PasswordInput");
            confirmPasswordInput = root.Q<TextField>("ConfirmPasswordInput");
            emailInput = root.Q<TextField>("EmailInput");
            registerButton = root.Q<Button>("RegisterButton");
            goToLoginButton = root.Q<Button>("GoToLoginButton");
            errorLabel = root.Q<Label>("ErrorLabel");

            // Configurar eventos
            if (registerButton != null)
                registerButton.clicked += OnRegisterClicked;

            if (goToLoginButton != null)
                goToLoginButton.clicked += OnGoToLoginClicked;

            // Permitir Enter para enviar
            if (confirmPasswordInput != null)
            {
                confirmPasswordInput.RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
                        OnRegisterClicked();
                });
            }

            // Ocultar por defecto
            Hide();
        }

        private void OnDisable()
        {
            if (registerButton != null)
                registerButton.clicked -= OnRegisterClicked;

            if (goToLoginButton != null)
                goToLoginButton.clicked -= OnGoToLoginClicked;
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Crear cuenta".
        /// Valida las entradas y realiza la petición de registro a la API.
        /// </summary>
        private void OnRegisterClicked()
        {
            if (!ValidateInputs())
                return;

            string username = userInput.value?.Trim();
            string password = passwordInput.value;
            string email = emailInput?.value?.Trim();

            // Email es opcional, enviar null si esta vacio
            if (string.IsNullOrEmpty(email))
                email = null;

            SetLoading(true);
            HideError();

            TriskelAPIClient.Instance.RegisterPlayer(
                username,
                password,
                email,
                response =>
                {
                    Debug.Log($"[RegisterController] Registro exitoso: {response.username}");
                    SetLoading(false);
                    ClearFields();
                    OnRegisterSuccess?.Invoke();
                },
                error =>
                {
                    Debug.LogWarning($"[RegisterController] Error registro: {error}");
                    SetLoading(false);
                    ShowError(ParseError(error));
                }
            );
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Ir a Login".
        /// Limpia los campos y dispara el evento OnGoToLogin.
        /// </summary>
        private void OnGoToLoginClicked()
        {
            ClearFields();
            OnGoToLogin?.Invoke();
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de registro.
        /// </summary>
        /// <returns>True si las entradas son válidas, False en caso contrario.</returns>
        /// <remarks>
        /// Valida:
        /// - Nombre de usuario: 3-20 caracteres
        /// - Contraseña: al menos 6 caracteres
        /// - Confirmación de contraseña: debe coincidir
        /// - Email (opcional): formato básico válido
        /// </remarks>
        private bool ValidateInputs()
        {
            string username = userInput?.value?.Trim();
            string password = passwordInput?.value;
            string confirmPassword = confirmPasswordInput?.value;

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

            if (username.Length > 20)
            {
                ShowError("El nombre no puede tener mas de 20 caracteres");
                return false;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Por favor, ingresa una contraseña");
                return false;
            }

            if (password.Length < 6)
            {
                ShowError("La contraseña debe tener al menos 6 caracteres");
                return false;
            }

            if (password != confirmPassword)
            {
                ShowError("Las contraseñas no coinciden");
                return false;
            }

            // Validar email si se proporciono
            string email = emailInput?.value?.Trim();
            if (!string.IsNullOrEmpty(email) && !IsValidEmail(email))
            {
                ShowError("El email no es valido");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida el formato básico de un email.
        /// </summary>
        /// <param name="email">Email a validar.</param>
        /// <returns>True si el email tiene un formato válido básico.</returns>
        /// <remarks>
        /// Validación simple que verifica la presencia de '@' y '.'.
        /// </remarks>
        private bool IsValidEmail(string email)
        {
            // Validacion basica de email
            return email.Contains("@") && email.Contains(".");
        }

        /// <summary>
        /// Parsea mensajes de error de la API y los convierte en mensajes amigables para el usuario.
        /// </summary>
        /// <param name="error">Mensaje de error de la API.</param>
        /// <returns>Mensaje de error amigable en español.</returns>
        private string ParseError(string error)
        {
            if (error.Contains("400") || error.Contains("already exists"))
                return "Este nombre de usuario ya esta en uso";

            if (error.Contains("422") || error.Contains("validation"))
                return "Datos invalidos. Revisa los campos";

            if (error.Contains("timeout") || error.Contains("Timeout"))
                return "Error de conexion. Intenta de nuevo";

            if (error.Contains("Unable to connect") || error.Contains("No connection"))
                return "No se pudo conectar al servidor";

            return "Error al crear cuenta. Intenta de nuevo";
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
            if (registerButton != null)
            {
                registerButton.SetEnabled(!loading);
                registerButton.text = loading ? "..." : "Crear cuenta";
            }

            if (goToLoginButton != null)
                goToLoginButton.SetEnabled(!loading);

            if (userInput != null)
                userInput.SetEnabled(!loading);

            if (passwordInput != null)
                passwordInput.SetEnabled(!loading);

            if (confirmPasswordInput != null)
                confirmPasswordInput.SetEnabled(!loading);

            if (emailInput != null)
                emailInput.SetEnabled(!loading);
        }

        /// <summary>
        /// Limpia todos los campos del formulario de registro.
        /// </summary>
        private void ClearFields()
        {
            if (userInput != null)
                userInput.value = "";

            if (passwordInput != null)
                passwordInput.value = "";

            if (confirmPasswordInput != null)
                confirmPasswordInput.value = "";

            if (emailInput != null)
                emailInput.value = "";

            HideError();
        }

        /// <summary>
        /// Muestra el overlay de registro y limpia los campos del formulario.
        /// </summary>
        public void Show()
        {
            if (registerOverlay != null)
                registerOverlay.style.display = DisplayStyle.Flex;

            ClearFields();
        }

        /// <summary>
        /// Oculta el overlay de registro y limpia los campos del formulario.
        /// </summary>
        public void Hide()
        {
            if (registerOverlay != null)
                registerOverlay.style.display = DisplayStyle.None;

            ClearFields();
        }
    }
}
