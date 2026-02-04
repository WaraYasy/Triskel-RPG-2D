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

        // NOTA: Si GameConstants no está asignado, usa estos valores por defecto
        private float NormalFontSize => gameConstants != null ? gameConstants.dialogueFontSizeNormal : normalFontSize;
        private float LargeFontSize => gameConstants != null ? gameConstants.dialogueFontSizeLarge : largeFontSize;
        private float NormalFontSizeDyslexic => gameConstants != null ? gameConstants.dialogueFontSizeNormalDyslexic : normalFontSize - 4f;
        private float LargeFontSizeDyslexic => gameConstants != null ? gameConstants.dialogueFontSizeLargeDyslexic : largeFontSize - 6f;

        private float NormalOptionFontSize => gameConstants != null ? gameConstants.dialogueOptionSizeNormal : normalOptionFontSize;
        private float LargeOptionFontSize => gameConstants != null ? gameConstants.dialogueOptionSizeLarge : largeOptionFontSize;
        private float NormalOptionFontSizeDyslexic => gameConstants != null ? gameConstants.dialogueOptionSizeNormalDyslexic : normalOptionFontSize - 3f;
        private float LargeOptionFontSizeDyslexic => gameConstants != null ? gameConstants.dialogueOptionSizeLargeDyslexic : largeOptionFontSize - 4f;

        [Header("Referencias UI")]
        [SerializeField] private Image backgroundPanel;
        [SerializeField] private Image borderImage;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Button continueButton;

        [Header("Configuración de Fuente")]
        [SerializeField] private TMP_FontAsset dialogueFontAsset;
        [SerializeField] private TMP_FontAsset dyslexicFontAsset;

        public DialogueTheme CurrentTheme { get; private set; }

        private TMP_FontAsset ActiveFontAsset =>
            (SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont && dyslexicFontAsset != null)
                ? dyslexicFontAsset : dialogueFontAsset;

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
                SettingsManager.Instance.OnFontChanged += ApplyFont;
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
                SettingsManager.Instance.OnFontChanged -= ApplyFont;
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
                 SettingsManager.Instance.OnFontChanged -= ApplyFont;
                 SettingsManager.Instance.OnFontChanged += ApplyFont;

                 // Aplicar inicial
                 ApplyFontSize(SettingsManager.Instance.UseLargeText);
                 ApplyFont(SettingsManager.Instance.UseDyslexicFont);
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
            if (ActiveFontAsset == null) return;

            // Buscar dinámicamente todos los botones en la UI de diálogo
            Button[] allButtons = GetComponentsInChildren<Button>(true);

            foreach (var btn in allButtons)
            {
                if (btn == null) continue;
                var btnText = btn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.font = ActiveFontAsset;
                    btnText.fontSharedMaterial = ActiveFontAsset.material;
                }
            }
        }

        private void ApplyFont(bool useDyslexic)
        {
            var fontToApply = ActiveFontAsset;

            Debug.Log($"[DialogueThemeManager] ApplyFont llamado - UseDyslexic: {useDyslexic}, dyslexicFontAsset: {(dyslexicFontAsset != null ? dyslexicFontAsset.name : "NULL")}, dialogueFontAsset: {(dialogueFontAsset != null ? dialogueFontAsset.name : "NULL")}, Aplicando: {(fontToApply != null ? fontToApply.name : "NULL")}");

            if (fontToApply == null)
            {
                Debug.LogError("[DialogueThemeManager] ⚠️ ActiveFontAsset es NULL - no se puede aplicar fuente");
                return;
            }

            if (dialogueText != null)
            {
                dialogueText.font = fontToApply;
                dialogueText.fontSharedMaterial = fontToApply.material;
                Debug.Log($"[DialogueThemeManager] 🔍 dialogueText.font ahora es: {dialogueText.font.name}");
            }

            if (characterNameText != null)
            {
                characterNameText.font = fontToApply;
                characterNameText.fontSharedMaterial = fontToApply.material;
                Debug.Log($"[DialogueThemeManager] 🔍 characterNameText.font ahora es: {characterNameText.font.name}");
            }

            ApplyFontToAllOptions();

            // Reaplicar tamaño de fuente para asegurar que sea correcto con la nueva fuente
            ApplyFontSize(SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText);

            Debug.Log($"[DialogueThemeManager] ✓ Fuente aplicada: {fontToApply.name}");
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
                    if (ActiveFontAsset != null)
                    {
                        btnText.font = ActiveFontAsset;
                        btnText.fontSharedMaterial = ActiveFontAsset.material;
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
            bool dyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;

            // Calcular tamaños según fuente activa
            float mainTextSize = dyslexic
                ? (large ? LargeFontSizeDyslexic : NormalFontSizeDyslexic)
                : (large ? LargeFontSize : NormalFontSize);

            float optionSize = dyslexic
                ? (large ? LargeOptionFontSizeDyslexic : NormalOptionFontSizeDyslexic)
                : (large ? LargeOptionFontSize : NormalOptionFontSize);

            Debug.Log($"[DialogueThemeManager] 📏 Calculando tamaños - Large: {large}, Dyslexic: {dyslexic}, MainText: {mainTextSize}, Options: {optionSize}");

            // Aplicar tamaño al texto principal del diálogo
            if (dialogueText != null)
            {
                dialogueText.enableAutoSizing = false;
                dialogueText.fontSize = mainTextSize;
                Debug.Log($"[DialogueThemeManager] 📏 dialogueText.fontSize ahora es: {dialogueText.fontSize}");
            }

            // Aplicar tamaño al nombre del personaje (si existe)
            if (characterNameText != null)
            {
                characterNameText.enableAutoSizing = false;
                characterNameText.fontSize = mainTextSize;
            }

            // Aplicar tamaño Y fuente a las opciones del jugador (búsqueda dinámica)
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
                    if (ActiveFontAsset != null)
                    {
                        btnText.font = ActiveFontAsset;
                        btnText.fontSharedMaterial = ActiveFontAsset.material;
                    }
                }
            }

            // Forzar actualización al final
            if (dialogueText != null) dialogueText.ForceMeshUpdate();
            if (characterNameText != null) characterNameText.ForceMeshUpdate();
            foreach (var btn in allButtons)
            {
                var btnText = btn?.GetComponentInChildren<TMP_Text>();
                if (btnText != null) btnText.ForceMeshUpdate();
            }

            Debug.Log($"[DialogueThemeManager] ✓ Tamaño de fuente aplicado: {(large ? "Grande" : "Normal")} ({(dyslexic ? "Dislexia" : "Pixelada")}) (Diálogo: {mainTextSize}, Opciones: {optionSize})");
        }


        private void OnDialogueStart()
        {
            ShowUI();
            ApplyTheme(CurrentTheme);

            // Aplicar fuente y tamaño
            bool useDyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;
            bool useLarge = SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText;

            ApplyFont(useDyslexic);

            // Aplicar nuevamente con delay para sobrescribir cualquier cambio de Yarn Spinner
            StartCoroutine(ApplySettingsDelayed(useDyslexic, useLarge));
        }

        private System.Collections.IEnumerator ApplySettingsDelayed(bool useDyslexic, bool useLarge)
        {
            yield return new WaitForEndOfFrame();

            Debug.Log("[DialogueThemeManager] 🔄 Re-aplicando configuración después del frame");

            // Re-aplicar fuente
            var fontToApply = ActiveFontAsset;
            if (fontToApply != null)
            {
                if (dialogueText != null)
                {
                    dialogueText.font = fontToApply;
                    dialogueText.fontSharedMaterial = fontToApply.material;
                }
                if (characterNameText != null)
                {
                    characterNameText.font = fontToApply;
                    characterNameText.fontSharedMaterial = fontToApply.material;
                }
                ApplyFontToAllOptions();
            }

            // Re-aplicar tamaño
            ApplyFontSize(useLarge);

            Debug.Log("[DialogueThemeManager] ✓ Configuración re-aplicada");
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
