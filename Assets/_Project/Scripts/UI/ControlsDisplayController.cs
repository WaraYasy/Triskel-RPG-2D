// =======================================================================================
// Triskel RPG 2D - Controls Display Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador de la UI que muestra los controles del juego.
//              Se abre al interactuar con un objeto en el hub.
// =======================================================================================

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de controles del juego.
    /// </summary>
    public class ControlsDisplayController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private PauseController pauseController;

        private VisualElement rootElement;
        private VisualElement controlsOverlay;
        private Button closeButton;

        private bool isVisible = false;

        private void OnEnable()
        {
            if (uiDocument == null)
                uiDocument = GetComponent<UIDocument>();

            if (uiDocument == null)
            {
                Debug.LogError("[ControlsDisplay] UIDocument no asignado");
                return;
            }

            rootElement = uiDocument.rootVisualElement;

            // Obtener referencias
            controlsOverlay = rootElement.Q<VisualElement>("ControlsOverlay");
            closeButton = rootElement.Q<Button>("CloseButton");

            // Configurar eventos
            if (closeButton != null)
                closeButton.clicked += OnCloseClicked;

            // Ocultar por defecto
            Hide();
        }

        private void OnDisable()
        {
            if (closeButton != null)
                closeButton.clicked -= OnCloseClicked;
        }

        private void Update()
        {
            // Permitir cerrar con ESC
            if (isVisible && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnCloseClicked();
            }
        }

        /// <summary>
        /// Muestra la UI de controles y pausa el juego.
        /// </summary>
        public void Show()
        {
            if (controlsOverlay == null) return;

            controlsOverlay.style.display = DisplayStyle.Flex;
            isVisible = true;

            // Pausar el juego
            Time.timeScale = 0f;

            Debug.Log("[ControlsDisplay] UI de controles mostrada");
        }

        /// <summary>
        /// Oculta la UI de controles y reanuda el juego.
        /// </summary>
        public void Hide()
        {
            if (controlsOverlay == null) return;

            controlsOverlay.style.display = DisplayStyle.None;
            isVisible = false;

            // Reanudar el juego
            Time.timeScale = 1f;

            Debug.Log("[ControlsDisplay] UI de controles ocultada");
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón Cerrar.
        /// Vuelve al menú de pausa si estaba pausado, o reanuda el juego si fue abierto desde el mundo.
        /// </summary>
        private void OnCloseClicked()
        {
            if (!isVisible) return;

            controlsOverlay.style.display = DisplayStyle.None;
            isVisible = false;

            // Buscar pauseController si no está asignado
            if (pauseController == null)
            {
                pauseController = FindFirstObjectByType<PauseController>();
            }

            // Si fue abierto desde el menú de pausa, volver a mostrar pausa
            if (pauseController != null && pauseController.IsPaused)
            {
                pauseController.Show();
                Debug.Log("[ControlsDisplay] Volviendo al menú de pausa");
            }
            else
            {
                // Si no, reanudar el juego (fue abierto desde el mundo)
                Time.timeScale = 1f;
                Debug.Log("[ControlsDisplay] Reanudando juego");
            }
        }

        /// <summary>
        /// Verifica si la UI está visible.
        /// </summary>
        public bool IsVisible => isVisible;
    }
}
