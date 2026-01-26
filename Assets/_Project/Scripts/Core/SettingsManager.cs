using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Manager centralizado para las configuraciones del juego.
    /// Maneja persistencia de audio y preferencias de UI.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        // Constantes para PlayerPrefs
        private const string KEY_MUSIC = "Settings_MusicVolume";
        private const string KEY_SFX = "Settings_SFXVolume";
        private const string KEY_FONT_SIZE = "Settings_LargeText";

        // Valores por defecto
        private const float DEFAULT_VOLUME = 0.8f;
        private const bool DEFAULT_LARGE_TEXT = false;

        // Propiedades públicas
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public bool UseLargeText { get; private set; }

        // Eventos para notificar cambios
        public event System.Action<float> OnMusicVolumeChanged;
        public event System.Action<float> OnSFXVolumeChanged;
        public event System.Action<bool> OnFontSizeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LoadSettings()
        {
            MusicVolume = PlayerPrefs.GetFloat(KEY_MUSIC, DEFAULT_VOLUME);
            SFXVolume = PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_VOLUME);
            UseLargeText = PlayerPrefs.GetInt(KEY_FONT_SIZE, DEFAULT_LARGE_TEXT ? 1 : 0) == 1;

            Debug.Log($"[SettingsManager] Cargado. Music: {MusicVolume:P0}, SFX: {SFXVolume:P0}, LargeText: {UseLargeText}");
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(KEY_MUSIC, MusicVolume);
            PlayerPrefs.Save();
            OnMusicVolumeChanged?.Invoke(MusicVolume);
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(KEY_SFX, SFXVolume);
            PlayerPrefs.Save();
            OnSFXVolumeChanged?.Invoke(SFXVolume);
        }

        public void SetLargeText(bool enable)
        {
            UseLargeText = enable;
            PlayerPrefs.SetInt(KEY_FONT_SIZE, enable ? 1 : 0);
            PlayerPrefs.Save();
            OnFontSizeChanged?.Invoke(UseLargeText);
            
            Debug.Log($"[SettingsManager] Texto Grande: {UseLargeText}");
        }
    }
}
