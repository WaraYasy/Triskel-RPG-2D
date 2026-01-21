using System;
using UnityEngine;
using UnityEngine.UIElements;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de Registro.
    /// Maneja la creacion de nuevas cuentas.
    /// </summary>
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

        // Eventos para navegacion
        public event Action OnGoToLogin;
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

            // Ocultar error inicialmente
            HideError();
        }

        private void OnDisable()
        {
            if (registerButton != null)
                registerButton.clicked -= OnRegisterClicked;

            if (goToLoginButton != null)
                goToLoginButton.clicked -= OnGoToLoginClicked;
        }

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
                    Hide();
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

        private void OnGoToLoginClicked()
        {
            Hide();
            OnGoToLogin?.Invoke();
        }

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

        private bool IsValidEmail(string email)
        {
            // Validacion basica de email
            return email.Contains("@") && email.Contains(".");
        }

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

        public void Show()
        {
            if (registerOverlay != null)
                registerOverlay.style.display = DisplayStyle.Flex;

            // Limpiar campos
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

        public void Hide()
        {
            if (registerOverlay != null)
                registerOverlay.style.display = DisplayStyle.None;

            // Limpiar passwords por seguridad
            if (passwordInput != null)
                passwordInput.value = "";

            if (confirmPasswordInput != null)
                confirmPasswordInput.value = "";
        }
    }
}
