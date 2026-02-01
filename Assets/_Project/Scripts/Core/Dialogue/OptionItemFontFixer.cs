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

        [Header("Tamaños de Texto")]
        [SerializeField] private float normalFontSize = 40f;
        [SerializeField] private float largeFontSize = 52f;

        private TMP_Text textComponent;

        private void Start()
        {
            // Buscar el TMP_Text en este objeto o en sus hijos
            textComponent = GetComponentInChildren<TMP_Text>();

            if (textComponent == null)
            {
                Debug.LogWarning("[OptionItemFontFixer] No se encontró TMP_Text en la opción");
                return;
            }

            // Aplicar fuente
            if (fontAsset != null)
            {
                textComponent.font = fontAsset;
            }

            // Aplicar tamaño inicial
            ApplyFontSize();

            // Suscribirse a cambios de tamaño
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged += OnFontSizeChanged;
            }
        }

        private void OnDestroy()
        {
            // Desuscribirse al destruir
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnFontSizeChanged -= OnFontSizeChanged;
            }
        }

        private void OnFontSizeChanged(bool useLargeText)
        {
            ApplyFontSize();
        }

        private void ApplyFontSize()
        {
            if (textComponent == null) return;

            bool useLarge = SettingsManager.Instance != null && SettingsManager.Instance.UseLargeText;
            textComponent.enableAutoSizing = false;
            textComponent.fontSize = useLarge ? largeFontSize : normalFontSize;
        }
    }
}
