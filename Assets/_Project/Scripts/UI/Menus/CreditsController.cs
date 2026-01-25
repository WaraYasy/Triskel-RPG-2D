using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador de la pantalla de créditos.
    /// Lee datos de un archivo JSON y los muestra con scroll automático.
    /// </summary>
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
        [System.Serializable]
        private class CreditsData
        {
            public string title;
            public List<CreditSection> sections;
            public string footer;
        }

        [System.Serializable]
        private class CreditSection
        {
            public string title;
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

        private void OnSkipClicked()
        {
            SkipCredits();
        }

        private void SkipCredits()
        {
            Debug.Log("[CreditsController] Saltando créditos...");
            isScrolling = false;
            StopAllCoroutines();
            LoadNextScene();
        }

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
        /// Permite cambiar la velocidad de scroll en tiempo de ejecución
        /// </summary>
        public void SetScrollSpeed(float speed)
        {
            scrollSpeed = Mathf.Max(0f, speed);
        }

        /// <summary>
        /// Permite cambiar la escena de destino
        /// </summary>
        public void SetNextScene(string sceneName)
        {
            nextSceneName = sceneName;
        }

        #endregion
    }
}
