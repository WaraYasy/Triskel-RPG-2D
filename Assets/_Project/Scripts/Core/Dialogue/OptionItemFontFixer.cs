// =======================================================================================
// Triskel RPG 2D - Option Item Font Fixer
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Componente que fuerza la fuente BoldPixels SDF en los textos de las opciones
//              de Yarn Spinner y actualiza el tamaño según la configuración del jugador.
// =======================================================================================

using UnityEngine;
using TMPro;
using Triskel.Core;

namespace Triskel.Dialogue
{
    /// <summary>
    /// Aplica la fuente y tamaño correctos a las opciones de Yarn Spinner.
    /// </summary>
    /// <remarks>
    /// Este componente debe añadirse al prefab personalizado de OptionItem.
    /// Escucha cambios en SettingsManager para actualizar el tamaño dinámicamente.
    /// </remarks>
    public class OptionItemFontFixer : MonoBehaviour
    {
        [Header("Configuración de Fuente")]
        [SerializeField] private TMP_FontAsset fontAsset;
        [SerializeField] private TMP_FontAsset dyslexicFontAsset;

        [Header("Tamaños de Texto")]
        [SerializeField] private float normalFontSize = 40f;
        [SerializeField] private float largeFontSize = 52f;

        private TMP_Text textComponent;

        private void Start()
        {
            textComponent = GetComponentInChildren<TMP_Text>();

            if (textComponent == null)
            {
                Debug.LogWarning("[OptionItemFontFixer] No se encontró TMP_Text en la opción");
                return;
            }

            ApplyFont();
            ApplyFontSize();

            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged += OnFontSizeChanged;
                SettingsManager.Instance.OnFontChanged += OnFontTypeChanged;
            }
        }

        private void OnDestroy()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged -= OnFontSizeChanged;
                SettingsManager.Instance.OnFontChanged -= OnFontTypeChanged;
            }
        }

        private void OnFontSizeChanged(bool useLargeText)
        {
            ApplyFontSize();
        }

        private void OnFontTypeChanged(bool useDyslexic)
        {
            ApplyFont();
        }

        private void ApplyFontSize()
        {
            if (textComponent == null) return;

            bool useLarge = SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText;
            textComponent.enableAutoSizing = false;
            textComponent.fontSize = useLarge ? largeFontSize : normalFontSize;
        }

        private void ApplyFont()
        {
            if (textComponent == null) return;

            bool useDyslexic = SettingsManager.Instance != null && SettingsManager.Instance.UseDyslexicFont;
            TMP_FontAsset targetFont = (useDyslexic && dyslexicFontAsset != null) ? dyslexicFontAsset : fontAsset;

            if (targetFont != null)
            {
                textComponent.font = targetFont;
                textComponent.fontSharedMaterial = targetFont.material;

                Debug.Log($"[OptionItemFontFixer] Fuente aplicada: {targetFont.name} (Dislexia: {useDyslexic})");
            }
        }
    }
}
