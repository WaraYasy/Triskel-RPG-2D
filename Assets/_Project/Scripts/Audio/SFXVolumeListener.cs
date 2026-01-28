using UnityEngine;
using Triskel.Core;

namespace Triskel.Audio
{
    /// <summary>
    /// Componente auxiliar que ajusta el volumen del AudioSource según el volumen global de SFX.
    /// Añadir a cualquier objeto que emita sonidos (pasos, UI, etc).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SFXVolumeListener : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Volumen base relativo a este objeto (0-1). Se multiplicará por el volumen global.")]
        [Range(0f, 1f)]
        [SerializeField] private float baseVolume = 1f;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSFXVolumeChanged += UpdateVolume;
                // Inicializar
                UpdateVolume(SettingsManager.Instance.SFXVolume);
            }
        }

        private void OnDisable()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.OnSFXVolumeChanged -= UpdateVolume;
            }
        }

        public void UpdateVolume(float globalVolume)
        {
            if (audioSource != null)
            {
                audioSource.volume = baseVolume * globalVolume;
            }
        }

        public void SetBaseVolume(float volume)
        {
            baseVolume = Mathf.Clamp01(volume);
            if (SettingsManager.Instance != null)
            {
                UpdateVolume(SettingsManager.Instance.SFXVolume);
            }
        }
    }
}
