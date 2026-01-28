using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace Triskel.UI
{
    /// <summary>
    /// Controlador del video introductorio.
    /// Reproduce un video y luego carga la escena del menú principal.
    /// El usuario puede saltar el video con cualquier tecla o click.
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class IntroVideoController : MonoBehaviour
    {
        [Header("Configuración")]
        [SerializeField] private string nextSceneName = "Home";
        [SerializeField] private bool allowSkip = true;
        [SerializeField] private float skipDelay = 1f; // Segundos antes de poder saltar
        
        [Header("Video")]
        [SerializeField] private VideoClip introVideo;
        
        [Header("Fade (Opcional)")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        
        [Header("Loading (Opcional)")]
        [SerializeField] private GameObject loadingIndicator; // Mostrar mientras carga
        
        private VideoPlayer videoPlayer;
        private bool canSkip = false;
        private bool isTransitioning = false;
        private float timer = 0f;
        
        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            
            // Configurar VideoPlayer
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = Camera.main;
            videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
            
            // Suscribirse al evento de fin de video
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        
        private void Start()
        {
            // Asignar video si está configurado
            if (introVideo != null)
            {
                videoPlayer.clip = introVideo;
            }
            
            // Iniciar fade in
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
            }
            
            // Preparar y reproducir video con corrutina
            StartCoroutine(PrepareAndPlayVideo());
        }
        
        private System.Collections.IEnumerator PrepareAndPlayVideo()
        {
            Debug.Log("[IntroVideoController] Preparando video...");
            
            // Mostrar indicador de carga (o pantalla negra si hay fadeCanvasGroup)
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);
            
            // Mantener pantalla negra durante la carga
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = 1f;
            
            // Configurar para no saltar frames (evita el "stuttering")
            videoPlayer.skipOnDrop = false;
            
            // Preparar el video
            videoPlayer.Prepare();
            
            // Esperar a que esté preparado
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
            
            Debug.Log("[IntroVideoController] Video preparado. Iniciando buffering...");
            
            // Reproducir el video (pero mantener oculto)
            videoPlayer.Play();
            
            // Pausar inmediatamente para dejar que cargue frames en buffer
            videoPlayer.Pause();
            
            // Esperar un momento para que se llene el buffer
            yield return new WaitForSecondsRealtime(0.5f);
            
            // Reanudar desde el principio
            videoPlayer.time = 0;
            videoPlayer.Play();
            
            // Esperar a que haya frames visibles
            while (videoPlayer.frame < 1)
            {
                yield return null;
            }
            
            Debug.Log("[IntroVideoController] Buffer listo. Mostrando video...");
            
            // Ocultar indicador de carga
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);
            
            // Mostrar video inmediatamente (sin fade para evitar más delay)
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }
        }
        
        private void Update()
        {
            // Contador para permitir saltar
            if (!canSkip && allowSkip)
            {
                timer += Time.deltaTime;
                if (timer >= skipDelay)
                {
                    canSkip = true;
                }
            }
            
            // Detectar input para saltar
            if (canSkip && !isTransitioning)
            {
                bool skipPressed = false;
                
                // Teclado (nuevo Input System)
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    skipPressed = true;
                }
                
                // Mouse
                if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    skipPressed = true;
                }
                
                // Gamepad
                if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
                {
                    skipPressed = true;
                }
                
                // Pantalla táctil
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                {
                    skipPressed = true;
                }
                
                if (skipPressed)
                {
                    SkipVideo();
                }
            }
        }
        
        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }
        
        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[IntroVideoController] Video terminado");
            LoadNextScene();
        }
        
        private void SkipVideo()
        {
            Debug.Log("[IntroVideoController] Video saltado por el usuario");
            videoPlayer.Stop();
            LoadNextScene();
        }
        
        private void LoadNextScene()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            
            if (fadeCanvasGroup != null)
            {
                StartCoroutine(FadeOutAndLoad());
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - (elapsed / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }
        
        private System.Collections.IEnumerator FadeOutAndLoad()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = elapsed / fadeDuration;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
            
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
