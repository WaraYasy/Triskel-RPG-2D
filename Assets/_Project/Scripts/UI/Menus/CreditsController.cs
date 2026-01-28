// =======================================================================================
// Triskel RPG 2D - Credits Controller
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Controlador de la pantalla de créditos del juego. Lee los créditos desde
//              un archivo JSON y los muestra con scroll automático. Permite al jugador
//              saltar los créditos presionando ESC o haciendo clic en un botón.
// =======================================================================================

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de créditos del juego.
    /// </summary>
    /// <remarks>
    /// Este controlador lee los créditos desde un archivo JSON estructurado y los muestra
    /// con scroll automático vertical. Los créditos se organizan en secciones con títulos
    /// y entradas.
    ///
    /// El jugador puede saltar los créditos presionando ESC o haciendo clic en el botón "Saltar".
    /// Al finalizar, carga automáticamente la escena configurada (normalmente el menú principal).
    /// </remarks>
    public class CreditsController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private UIDocument creditsDocument;
        [SerializeField] private TextAsset creditsJsonFile;

        [Header("Configuración de Scroll")]
        [SerializeField] private float scrollSpeed = 50f; // Píxeles por segundo
        [SerializeField] private float initialDelay = 1f; // Segundos antes de empezar a scrollear
        [SerializeField] private float endDelay = 2f; // Segundos de pausa al final

        [Header("Escenas")]
        [SerializeField] private string nextSceneName = "Home"; // Escena a cargar al finalizar

        // Elementos UI
        private ScrollView scrollView;
        private VisualElement creditsContainer;
        private Label titleLabel;
        private Label footerLabel;
        private Button skipButton;

        // Estado
        private bool isScrolling = false;
        private CreditsData creditsData;

        // Datos del JSON
        /// <summary>
        /// Estructura de datos para los créditos cargados desde JSON.
        /// </summary>
        [System.Serializable]
        private class CreditsData
        {
            /// <summary>Título principal de los créditos.</summary>
            public string title;
            /// <summary>Lista de secciones de créditos.</summary>
            public List<CreditSection> sections;
            /// <summary>Texto del pie de página.</summary>
            public string footer;
        }

        /// <summary>
        /// Estructura de datos para una sección de créditos.
        /// </summary>
        [System.Serializable]
        private class CreditSection
        {
            /// <summary>Título de la sección (ej: "Desarrollo", "Arte", "Música").</summary>
            public string title;
            /// <summary>Lista de entradas de la sección (nombres, roles, etc.).</summary>
            public List<string> entries;
        }

        private void OnEnable()
        {
            InitializeCredits();
            LoadCreditsData();
            StartCoroutine(AutoScrollRoutine());
        }

        private void OnDisable()
        {
            if (skipButton != null)
                skipButton.clicked -= OnSkipClicked;
        }

        private void Update()
        {
            // Permitir saltar con ESC
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SkipCredits();
            }
        }

        /// <summary>
        /// Inicializa la pantalla de créditos obteniendo referencias a elementos UI.
        /// </summary>
        private void InitializeCredits()
        {
            if (creditsDocument == null)
                creditsDocument = GetComponent<UIDocument>();

            if (creditsDocument == null)
            {
                Debug.LogError("[CreditsController] UIDocument no asignado");
                return;
            }

            var root = creditsDocument.rootVisualElement;

            // Obtener referencias
            scrollView = root.Q<ScrollView>("CreditsScrollView");
            creditsContainer = root.Q<VisualElement>("CreditsContainer");
            titleLabel = root.Q<Label>("CreditsTitle");
            footerLabel = root.Q<Label>("CreditsFooter");
            skipButton = root.Q<Button>("SkipButton");

            // Configurar eventos
            if (skipButton != null)
                skipButton.clicked += OnSkipClicked;

            // Configurar ScrollView para scroll vertical solamente
            if (scrollView != null)
            {
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            }
        }

        /// <summary>
        /// Carga los datos de créditos desde el archivo JSON asignado.
        /// </summary>
        private void LoadCreditsData()
        {
            if (creditsJsonFile == null)
            {
                Debug.LogError("[CreditsController] Archivo JSON de créditos no asignado");
                return;
            }

            try
            {
                creditsData = JsonUtility.FromJson<CreditsData>(creditsJsonFile.text);
                PopulateCredits();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[CreditsController] Error al cargar créditos: {e.Message}");
            }
        }

        /// <summary>
        /// Rellena el contenedor de créditos con los datos cargados del JSON.
        /// </summary>
        /// <remarks>
        /// Crea dinámicamente Labels para el título, secciones, entradas y footer,
        /// aplicando las clases CSS apropiadas para el estilo.
        /// </remarks>
        private void PopulateCredits()
        {
            if (creditsData == null || creditsContainer == null)
                return;

            // Limpiar contenedor
            creditsContainer.Clear();

            // Agregar título principal
            if (titleLabel != null)
                titleLabel.text = creditsData.title;

            // Agregar secciones
            foreach (var section in creditsData.sections)
            {
                // Título de sección
                var sectionTitle = new Label(section.title);
                sectionTitle.AddToClassList("credits-section-title");
                creditsContainer.Add(sectionTitle);

                // Entradas de la sección
                foreach (var entry in section.entries)
                {
                    var entryLabel = new Label(entry);
                    entryLabel.AddToClassList("credits-entry");
                    creditsContainer.Add(entryLabel);
                }

                // Espaciador entre secciones
                var spacer = new VisualElement();
                spacer.AddToClassList("credits-spacer");
                creditsContainer.Add(spacer);
            }

            // Agregar footer
            if (footerLabel != null)
                footerLabel.text = creditsData.footer;

            Debug.Log("[CreditsController] Créditos cargados exitosamente");
        }

        /// <summary>
        /// Corrutina que realiza el scroll automático de los créditos.
        /// </summary>
        /// <returns>IEnumerator para la corrutina.</returns>
        /// <remarks>
        /// Espera un delay inicial, luego scrollea automáticamente a la velocidad configurada,
        /// espera un delay al final y finalmente carga la escena siguiente.
        /// </remarks>
        private IEnumerator AutoScrollRoutine()
        {
            if (scrollView == null)
                yield break;

            // Delay inicial
            yield return new WaitForSeconds(initialDelay);

            isScrolling = true;
            float targetScroll = scrollView.contentContainer.layout.height;
            float currentScroll = 0f;

            // Empezar desde abajo (mostrar contenido desde el principio)
            scrollView.scrollOffset = new Vector2(0, 0);

            // Scroll automático
            while (currentScroll < targetScroll && isScrolling)
            {
                currentScroll += scrollSpeed * Time.deltaTime;
                scrollView.scrollOffset = new Vector2(0, currentScroll);
                yield return null;
            }

            // Delay al final
            yield return new WaitForSeconds(endDelay);

            // Volver a la escena siguiente
            if (isScrolling)
                LoadNextScene();
        }

        /// <summary>
        /// Callback cuando se hace clic en el botón "Saltar".
        /// </summary>
        private void OnSkipClicked()
        {
            SkipCredits();
        }

        /// <summary>
        /// Detiene el scroll automático y carga la escena siguiente.
        /// </summary>
        private void SkipCredits()
        {
            Debug.Log("[CreditsController] Saltando créditos...");
            isScrolling = false;
            StopAllCoroutines();
            LoadNextScene();
        }

        /// <summary>
        /// Carga la escena configurada como siguiente (normalmente el menú principal).
        /// </summary>
        private void LoadNextScene()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[CreditsController] Cargando escena: {nextSceneName}");
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[CreditsController] No hay escena siguiente configurada");
            }
        }

        #region Public Methods

        /// <summary>
        /// Permite cambiar la velocidad de scroll en tiempo de ejecución.
        /// </summary>
        /// <param name="speed">Nueva velocidad de scroll en píxeles por segundo.</param>
        public void SetScrollSpeed(float speed)
        {
            scrollSpeed = Mathf.Max(0f, speed);
        }

        /// <summary>
        /// Permite cambiar la escena de destino al finalizar los créditos.
        /// </summary>
        /// <param name="sceneName">Nombre de la escena a cargar.</param>
        public void SetNextScene(string sceneName)
        {
            nextSceneName = sceneName;
        }

        #endregion
    }
}
