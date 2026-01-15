using UnityEngine;

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
            audioSource.volume = volume;
        }
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
        // Fade in suave
        if (useFadeIn && audioSource.isPlaying && audioSource.volume < targetVolume)
        {
            audioSource.volume = Mathf.MoveTowards(
                audioSource.volume,
                targetVolume,
                (targetVolume / fadeInDuration) * Time.deltaTime
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
