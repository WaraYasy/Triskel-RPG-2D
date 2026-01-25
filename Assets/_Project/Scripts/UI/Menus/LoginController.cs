using System;
using UnityEngine;
using UnityEngine.UIElements;
using Triskel.API;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de Login.
    /// Funciona como overlay dentro del MainMenu.
    /// </summary>
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
        public event Action OnGoToRegister;
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

        private void OnGoToRegisterClicked()
        {
            ClearFields();
            OnGoToRegister?.Invoke();
        }

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

            if (goToRegisterButton != null)
                goToRegisterButton.SetEnabled(!loading);

            if (userInput != null)
                userInput.SetEnabled(!loading);

            if (passwordInput != null)
                passwordInput.SetEnabled(!loading);
        }

        private void ClearFields()
        {
            if (userInput != null)
                userInput.value = "";

            if (passwordInput != null)
                passwordInput.value = "";

            HideError();
        }

        public void Show()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.Flex;

            ClearFields();
        }

        public void Hide()
        {
            if (loginOverlay != null)
                loginOverlay.style.display = DisplayStyle.None;

            ClearFields();
        }
    }
}
