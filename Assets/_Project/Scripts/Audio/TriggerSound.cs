using UnityEngine;
using Triskel.Audio;

namespace Triskel.Audio
{
    /// <summary>
    /// TriggerSound - Reproduce un sonido cuando el jugador pasa por el trigger.
    /// Versión 1.0 - Con control de volumen global y opciones de reproducción
    ///
    /// USO:
    /// 1. Añade este script a un GameObject
    /// 2. Añade un Collider2D y márcalo como "Is Trigger"
    /// 3. Asigna el AudioClip que quieras reproducir
    /// 4. Configura las opciones según tus necesidades
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(SFXVolumeListener))]
    public class TriggerSound : MonoBehaviour
    {
        [Header("Configuración de Sonido")]
        [Tooltip("Sonido a reproducir cuando el jugador pasa")]
        [SerializeField] private AudioClip soundClip;

        [Header("Opciones de Reproducción")]
        [Tooltip("Si TRUE, solo se reproduce una vez y luego se destruye")]
        [SerializeField] private bool playOnce = false;

        [Tooltip("Si TRUE, se reproduce automáticamente al entrar. Si FALSE, se reproduce al salir")]
        [SerializeField] private bool playOnEnter = true;

        [Tooltip("Tiempo de espera antes de destruir el objeto (si playOnce = true)")]
        [SerializeField] private float destroyDelay = 2f;

        [Header("Variación de Sonido (Opcional)")]
        [Tooltip("Variar volumen para más realismo")]
        [SerializeField] private bool randomizeVolume = false;
        [SerializeField] private float volumeMin = 0.8f;
        [SerializeField] private float volumeMax = 1.0f;

        [Tooltip("Variar pitch para más realismo")]
        [SerializeField] private bool randomizePitch = false;
        [SerializeField] private float pitchMin = 0.95f;
        [SerializeField] private float pitchMax = 1.05f;

        [Header("Cooldown (Opcional)")]
        [Tooltip("Tiempo mínimo entre reproducciones (0 = sin cooldown)")]
        [SerializeField] private float cooldownTime = 0f;

        // Referencias
        private AudioSource audioSource;
        private SFXVolumeListener volumeListener;
        private Collider2D triggerCollider;

        // Estado
        private bool hasPlayed = false;
        private float lastPlayTime = -999f;

        #region Unity Lifecycle

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            volumeListener = GetComponent<SFXVolumeListener>();
            triggerCollider = GetComponent<Collider2D>();

            // Configurar AudioSource
            audioSource.playOnAwake = false;
            audioSource.clip = soundClip;
            audioSource.spatialBlend = 0f; // Sonido 2D (ajusta si quieres 3D)
        }

        private void OnValidate()
        {
            // Auto-configurar collider como trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.Log($"[TriggerSound] Collider de '{gameObject.name}' configurado como Trigger.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (playOnEnter && other.CompareTag("Player"))
            {
                TryPlaySound();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!playOnEnter && other.CompareTag("Player"))
            {
                TryPlaySound();
            }
        }

        #endregion

        #region Sound Logic

        /// <summary>
        /// Intenta reproducir el sonido si se cumplen las condiciones.
        /// </summary>
        private void TryPlaySound()
        {
            // Verificar si ya se reprodujo (si solo debe sonar una vez)
            if (playOnce && hasPlayed)
            {
                return;
            }

            // Verificar cooldown
            if (Time.time - lastPlayTime < cooldownTime)
            {
                return;
            }

            // Verificar que haya un clip asignado
            if (soundClip == null)
            {
                Debug.LogWarning($"[TriggerSound] '{gameObject.name}' no tiene soundClip asignado.");
                return;
            }

            PlaySound();
        }

        /// <summary>
        /// Reproduce el sonido con las configuraciones establecidas.
        /// </summary>
        private void PlaySound()
        {
            // Guardar valores originales
            float originalVolume = audioSource.volume;
            float originalPitch = audioSource.pitch;

            // Aplicar variaciones temporales si están habilitadas
            if (randomizeVolume)
            {
                audioSource.volume = originalVolume * Random.Range(volumeMin, volumeMax);
            }

            if (randomizePitch)
            {
                audioSource.pitch = Random.Range(pitchMin, pitchMax);
            }

            // Reproducir sonido
            audioSource.PlayOneShot(soundClip);

            // Restaurar valores (importante para PlayOneShot)
            audioSource.volume = originalVolume;
            audioSource.pitch = originalPitch;

            // Actualizar estado
            hasPlayed = true;
            lastPlayTime = Time.time;

            Debug.Log($"[TriggerSound] Reproduciendo: {soundClip.name}");

            // Si solo se reproduce una vez, destruir después del delay
            if (playOnce)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resetea el estado para permitir que el sonido se reproduzca de nuevo.
        /// Útil si playOnce = true pero quieres reutilizar el trigger.
        /// </summary>
        public void ResetTrigger()
        {
            hasPlayed = false;
            lastPlayTime = -999f;
        }

        /// <summary>
        /// Fuerza la reproducción del sonido ignorando condiciones.
        /// </summary>
        public void ForcePlay()
        {
            if (soundClip != null)
            {
                audioSource.PlayOneShot(soundClip);
            }
        }

        #endregion
    }
}
