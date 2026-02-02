using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.InputSystem;

namespace Triskel.UI.Menus
{
    /// <summary>
    /// Controla la reproducción del video introductorio y la transición al menú principal.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    [RequireComponent(typeof(AudioSource))]
    public class IntroVideoController : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Nombre de la escena a cargar al finalizar el video")]
        [SerializeField] private string nextSceneName = "Home"; // Asumimos que Home es el menú principal, el usuario puede cambiarlo
        
        [Tooltip("¿Permitir saltar el video con clic o tecla?")]
        [SerializeField] private bool allowSkip = true;

        private VideoPlayer videoPlayer;
        private AudioSource audioSource;
        private bool isTransitioning = false;

        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            audioSource = GetComponent<AudioSource>();

            // Configurar AudioSource
            audioSource.playOnAwake = false;

            // Configurar VideoPlayer para usar AudioSource (mejor sincronización y buffering)
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }

        private void Start()
        {
            // Configuración inicial para evitar congelamientos
            videoPlayer.playOnAwake = false; // Controlamos nosotros el inicio
            videoPlayer.isLooping = false;
            
            // Suscribirse a eventos
            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            
            // Iniciar preparación del video (buffering)
            Debug.Log("[Intro] Preparando video...");
            videoPlayer.Prepare();
        }

        private void OnVideoPrepared(VideoPlayer vp)
        {
            Debug.Log("[Intro] Video preparado. Iniciando reproducción.");
            vp.Play();
        }

        private void Update()
        {
            if (allowSkip && !isTransitioning)
            {
                // Detectar input para saltar usando el nuevo Input System
                // Comprobamos si el teclado o pointer (mouse/touch) existen y se han pulsado
                bool skipRequested = false;

                if (Keyboard.current != null)
                {
                    if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
                        skipRequested = true;
                }

                if (Pointer.current != null)
                {
                    if (Pointer.current.press.wasPressedThisFrame)
                        skipRequested = true;
                }

                if (skipRequested)
                {
                    Debug.Log("[Intro] Video saltado por el usuario.");
                    LoadNextScene();
                }
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[Intro] Video finalizado.");
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            if (isTransitioning) return;
            isTransitioning = true;

            Debug.Log($"[Intro] Cargando siguiente escena: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
