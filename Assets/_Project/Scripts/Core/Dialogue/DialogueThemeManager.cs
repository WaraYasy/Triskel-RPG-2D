using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

using Triskel.Core;

namespace Triskel.Dialogue
{
    /// <summary>
    /// Gestiona el tema claro/oscuro del sistema de diálogos.
    /// </summary>
    public class DialogueThemeManager : MonoBehaviour
    {
        private const string THEME_PREF_KEY = "DialogueTheme";

        [Header("Configuración")]
        [SerializeField] private GameConstants gameConstants;

        [Header("Referencias UI")]
        [SerializeField] private Image backgroundPanel;
        [SerializeField] private Image borderImage;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Button continueButton;

        [Header("Configuración de Fuente")]
        [SerializeField] private TMP_FontAsset dialogueFontAsset;

        public DialogueTheme CurrentTheme { get; private set; }

        [Header("Tamaño de Texto")]
        [SerializeField] private float normalFontSize = 48f;
        [SerializeField] private float largeFontSize = 64f;
        [SerializeField] private float normalOptionFontSize = 36f;
        [SerializeField] private float largeOptionFontSize = 48f;

        [Header("Visibilidad y Control")]
        [SerializeField] private DialogueRunner dialogueRunner;
        [SerializeField] private CanvasGroup uiCanvasGroup;
        [SerializeField] private GameObject uiRootObject; // Alternativa si no hay CanvasGroup

        private void OnEnable()
        {
            if (dialogueRunner == null)
                dialogueRunner = FindFirstObjectByType<DialogueRunner>();

            if (dialogueRunner != null)
            {
                dialogueRunner.onDialogueStart.AddListener(OnDialogueStart);
                dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged += ApplyFontSize;
            }
        }

        private void OnDisable()
        {
            if (dialogueRunner != null)
            {
                dialogueRunner.onDialogueStart.RemoveListener(OnDialogueStart);
                dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
            }

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged -= ApplyFontSize;
            }
        }

        private void Awake()
        {
            int saved = PlayerPrefs.GetInt(THEME_PREF_KEY, 0);
            CurrentTheme = (DialogueTheme)saved;
        }

        private void Start()
        {
            if (gameConstants == null)
            {
                Debug.LogError("[DialogueThemeManager] GameConstants no asignado.");
                // No retornamos aqui para permitir que el resto funcione
            }

            // Retry subscription if it failed in OnEnable (Race Condition fix)
            if (SettingsManager.Instance != null)
            {
                 // Asegurar no suscribirse doble
                 SettingsManager.Instance.OnFontSizeChanged -= ApplyFontSize;
                 SettingsManager.Instance.OnFontSizeChanged += ApplyFontSize;
                 
                 // Aplicar inicial
                 ApplyFontSize(SettingsManager.Instance.UseLargeText);
            }

            // Intentar encontrar referencias de UI si faltan
            if (uiCanvasGroup == null && backgroundPanel != null)
                uiCanvasGroup = backgroundPanel.GetComponentInParent<CanvasGroup>();
            
            if (uiRootObject == null && uiCanvasGroup != null)
                uiRootObject = uiCanvasGroup.gameObject;
            else if (uiRootObject == null && backgroundPanel != null)
                uiRootObject = backgroundPanel.transform.parent.gameObject;

            ApplyTheme(CurrentTheme);
            ApplyFontToAllOptions(); // Asegurar que la fuente se aplique al inicio

            // Estado inicial: Ocultar si no hay diálogo activo
            if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                OnDialogueComplete();
            }
        }

        /// <summary>
        /// Aplica el font asset configurado a todas las opciones del jugador.
        /// Útil para asegurar consistencia visual al iniciar.
        /// </summary>
        private void ApplyFontToAllOptions()
        {
            if (dialogueFontAsset == null) return;

            // Buscar dinámicamente todos los botones en la UI de diálogo
            Button[] allButtons = GetComponentsInChildren<Button>(true);

            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                var btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.font = dialogueFontAsset;
                }
            }
        }


        public void SetTheme(DialogueTheme theme)
        {
            CurrentTheme = theme;
            ApplyTheme(theme);
            PlayerPrefs.SetInt(THEME_PREF_KEY, (int)theme);
            PlayerPrefs.Save();
        }

        public void ToggleTheme()
        {
            SetTheme(CurrentTheme == DialogueTheme.Oscuro ? DialogueTheme.Claro : DialogueTheme.Oscuro);
        }

        private void ApplyTheme(DialogueTheme theme)
        {
            bool oscuro = theme == DialogueTheme.Oscuro;

            Color fondo = oscuro ? gameConstants.oscuro_Fondo : gameConstants.claro_Fondo;
            Color borde = oscuro ? gameConstants.oscuro_Borde : gameConstants.claro_Borde;
            Color texto = oscuro ? gameConstants.oscuro_Texto : gameConstants.claro_Texto;
            Color nombre = oscuro ? gameConstants.oscuro_NombrePersonaje : gameConstants.claro_NombrePersonaje;
            Color boton = oscuro ? gameConstants.oscuro_Boton : gameConstants.claro_Boton;
            Color botonHover = oscuro ? gameConstants.oscuro_BotonHover : gameConstants.claro_BotonHover;

            if (backgroundPanel != null) backgroundPanel.color = fondo;
            if (borderImage != null) borderImage.color = borde;
            if (dialogueText != null) dialogueText.color = texto;
            if (characterNameText != null) characterNameText.color = nombre;

            // Buscar dinámicamente todos los botones (porque Yarn Spinner los crea en runtime)
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                ApplyButtonColors(btn, boton, botonHover);
                var btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.color = texto;
                    // Aplicar el mismo font asset que el texto principal
                    if (dialogueFontAsset != null)
                    {
                        btnText.font = dialogueFontAsset;
                    }
                }
            }

            if (continueButton != null)
                ApplyButtonColors(continueButton, boton, botonHover);

            Debug.Log($"[DialogueThemeManager] Tema aplicado: {theme}");
        }

        private void ApplyButtonColors(Button button, Color normal, Color hover)
        {
            if (button.image != null) button.image.color = normal;

            var colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = hover;
            colors.pressedColor = hover * 0.9f;
            colors.selectedColor = hover;
            button.colors = colors;
        }

        private void ApplyFontSize(bool large)
        {
            // Aplicar tamaño al texto principal del diálogo
            if (dialogueText != null)
            {
                dialogueText.enableAutoSizing = false;
                dialogueText.fontSize = large ? largeFontSize : normalFontSize;
            }

            // Aplicar tamaño Y fuente a las opciones del jugador (búsqueda dinámica)
            float optionSize = large ? largeOptionFontSize : normalOptionFontSize;
            Button[] allButtons = GetComponentsInChildren<Button>(true);

            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                var btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.enableAutoSizing = false;
                    btnText.fontSize = optionSize;

                    // Aplicar el mismo font asset que el texto principal
                    if (dialogueFontAsset != null)
                    {
                        btnText.font = dialogueFontAsset;
                    }
                }
            }

            // Aplicar tamaño al nombre del personaje (si existe)
            if (characterNameText != null)
            {
                characterNameText.enableAutoSizing = false;
                characterNameText.fontSize = large ? largeFontSize : normalFontSize;
            }

            Debug.Log($"[DialogueThemeManager] Tamaño de fuente aplicado: {(large ? "Grande" : "Normal")} (Diálogo: {(dialogueText != null ? dialogueText.fontSize : 0)}, Opciones: {optionSize})");
        }


        private void OnDialogueStart()
        {
            ShowUI();
        }

        private void OnDialogueComplete()
        {
            HideUI();
        }

        public void ShowUI()
        {
            if (uiCanvasGroup != null)
            {
                uiCanvasGroup.alpha = 1f;
                uiCanvasGroup.interactable = true;
                uiCanvasGroup.blocksRaycasts = true;
            }
            else if (uiRootObject != null)
            {
                uiRootObject.SetActive(true);
            }
        }

        public void HideUI()
        {
            if (uiCanvasGroup != null)
            {
                uiCanvasGroup.alpha = 0f;
                uiCanvasGroup.interactable = false;
                uiCanvasGroup.blocksRaycasts = false;
            }
            else if (uiRootObject != null)
            {
                uiRootObject.SetActive(false);
            }
        }
    }
}
