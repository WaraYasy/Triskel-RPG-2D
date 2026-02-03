using UnityEngine;
using Triskel.Core;

/// <summary>
/// MusicManager - Gestiona música de fondo del nivel
/// Versión 1.0 - Con fade in/out y control de volumen
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    
    [Header("Fade (Opcional)")]
    [SerializeField] private bool useFadeIn = true;
    [SerializeField] private float fadeInDuration = 2f;
    
    private AudioSource audioSource;
    private float targetVolume;

    private void Awake()
    {
        // Crear AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D
        
        targetVolume = volume;
        
        if (useFadeIn)
        {
            audioSource.volume = 0f; // Empezar en silencio
        }
        else
        {
            // Inicializar con volumen global
            float globalVolume = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : volume;
            targetVolume = globalVolume * volume; // (Volumen Global * Volumen Local)
            audioSource.volume = targetVolume;
        }

        // Suscribirse a cambios de volumen
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged += OnGlobalVolumeChanged;
            
            // Actualizar targetVolume inmediatamente
            OnGlobalVolumeChanged(SettingsManager.Instance.MusicVolume);
        }
    }

    private void OnDestroy()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= OnGlobalVolumeChanged;
        }
    }

    private void OnGlobalVolumeChanged(float globalVolume)
    {
        // El volumen final es el volumen global (0-1) multiplicado por el volumen base de este clip (0-1)
        targetVolume = globalVolume * volume;

        // Si no estamos haciendo fade, aplicamos inmediatamente
        if (!useFadeIn)
        {
            audioSource.volume = targetVolume;
        }
        // Si useFadeIn está activo, el Update se encargará de ajustar el volumen suavemente
    }

    private void Start()
    {
        if (playOnAwake && backgroundMusic != null)
        {
            Play();
        }
    }

    private void Update()
    {
        // Fade in suave o ajuste de volumen
        if (useFadeIn && audioSource.isPlaying && audioSource.volume != targetVolume)
        {
            // Calcular velocidad de fade (más rápido para cambios de slider, más lento para fade inicial)
            // Usar unscaledDeltaTime para que funcione cuando el juego está pausado (ajustes)
            float fadeSpeed = Mathf.Abs(audioSource.volume - targetVolume) > 0.1f
                ? (1f / fadeInDuration) * Time.unscaledDeltaTime  // Fade inicial lento
                : 2f * Time.unscaledDeltaTime; // Cambios de slider más rápidos

            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                targetVolume,
                fadeSpeed
            );
        }
    }

    public void Play()
    {
        if (backgroundMusic != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log($"[MusicManager] Reproduciendo: {backgroundMusic.name}");
        }
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void Pause()
    {
        audioSource.Pause();
    }

    public void Resume()
    {
        audioSource.UnPause();
    }

    public void SetVolume(float newVolume)
    {
        targetVolume = Mathf.Clamp01(newVolume);
        if (!useFadeIn)
        {
            audioSource.volume = targetVolume;
        }
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = targetVolume; // Restaurar para próxima vez
    }
}
