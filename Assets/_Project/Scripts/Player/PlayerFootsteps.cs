using UnityEngine;
using Triskel.Core;

/// <summary>
/// PlayerFootsteps - Reproduce sonidos de pasos
/// Versión 1.0 - Con Animation Events para sincronización perfecta
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Sonidos de Pasos")]
    [SerializeField] private AudioClip[] footstepSounds;
    
    [Header("Configuración")]
    [SerializeField] private float volumeMin = 0.8f;
    [SerializeField] private float volumeMax = 1.0f;
    [SerializeField] private float pitchMin = 0.95f;
    [SerializeField] private float pitchMax = 1.05f;
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Llamar desde Animation Event
    /// Nombre del método DEBE ser exactamente: PlayFootstep
    /// </summary>
    public void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
        {
            Debug.LogWarning("[PlayerFootsteps] No hay sonidos asignados!");
            return;
        }
        
        // Seleccionar sonido aleatorio
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        
        // Variar volumen y pitch para más realismo, respetando volumen global de SFX
        float globalSFX = SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 1f;
        audioSource.volume = Random.Range(volumeMin, volumeMax) * globalSFX;
        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        
        // Reproducir
        audioSource.PlayOneShot(clip);
    }
}
