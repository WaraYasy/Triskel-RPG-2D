using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        public DialogueTheme CurrentTheme { get; private set; }

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
                return;
            }
            ApplyTheme(CurrentTheme);
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

            if (optionButtons != null)
            {
                foreach (var btn in optionButtons)
                {
                    if (btn == null) continue;
                    ApplyButtonColors(btn, boton, botonHover);
                    var btnText = btn.GetComponentInChildren<TMP_Text>();
                    if (btnText != null) btnText.color = texto;
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
    }
}
