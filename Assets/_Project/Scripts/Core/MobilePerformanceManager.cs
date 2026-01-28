using UnityEngine;

namespace Triskel.Core
{
    /// <summary>
    /// Gestor de rendimiento para móvil. Añade este script a un GameObject vacío en tu escena inicial.
    /// Se ejecuta antes que cualquier otro script y persiste entre escenas.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class MobilePerformanceManager : MonoBehaviour
    {
        private static MobilePerformanceManager instance;

        [Header("Frame Rate Settings")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool disableVSync = true;

        [Header("Quality Settings")]
        [SerializeField] private bool optimizeForMobile = true;
        [SerializeField] private bool reduceShadowDistance = true;
        [SerializeField] private float mobileShadowDistance = 20f;

        [Header("Memory Settings")]
        [SerializeField] private bool unloadUnusedAssets = true;
        [SerializeField] private float unloadInterval = 60f; // segundos

        [Header("Debug")]
        [SerializeField] private bool showFPS = false;

        private float deltaTime = 0.0f;
        private float unloadTimer = 0f;

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyPerformanceSettings();
        }

        private void ApplyPerformanceSettings()
        {
            // Frame rate
            Application.targetFrameRate = targetFrameRate;
            
            if (disableVSync)
            {
                QualitySettings.vSyncCount = 0;
            }

            if (optimizeForMobile)
            {
                // Optimizaciones de calidad para móvil
                QualitySettings.shadows = ShadowQuality.Disable; // Desactivar sombras en 2D
                QualitySettings.softParticles = false;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.billboardsFaceCameraPosition = false;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                
                // Reducir LOD
                QualitySettings.maximumLODLevel = 0;
                QualitySettings.lodBias = 1.0f;
                
                // Partículas
                QualitySettings.particleRaycastBudget = 64;

                // Skinned mesh
                QualitySettings.skinWeights = SkinWeights.TwoBones;
            }

            if (reduceShadowDistance)
            {
                QualitySettings.shadowDistance = mobileShadowDistance;
            }

            // Optimización de física 2D
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
            Physics2D.autoSyncTransforms = false; // Mejor rendimiento

            Debug.Log($"[MobilePerformanceManager] Configurado: {targetFrameRate} FPS, VSync: {!disableVSync}");
        }

        private void Update()
        {
            // FPS counter
            if (showFPS)
            {
                deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            }

            // Liberar memoria periódicamente
            if (unloadUnusedAssets)
            {
                unloadTimer += Time.unscaledDeltaTime;
                if (unloadTimer >= unloadInterval)
                {
                    unloadTimer = 0f;
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
        }

        private void OnGUI()
        {
            if (!showFPS) return;

            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(10, 10, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 3 / 100;
            style.normal.textColor = Color.white;

            float fps = 1.0f / deltaTime;
            string text = $"FPS: {fps:0.}";
            
            // Color según rendimiento
            if (fps >= 55) style.normal.textColor = Color.green;
            else if (fps >= 40) style.normal.textColor = Color.yellow;
            else style.normal.textColor = Color.red;

            GUI.Label(rect, text, style);
        }

        /// <summary>
        /// Llamar cuando hay poca memoria o al cambiar de escena
        /// </summary>
        public static void ForceMemoryCleanup()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.Log("[MobilePerformanceManager] Memoria liberada");
        }

        /// <summary>
        /// Establecer calidad baja para dispositivos lentos
        /// </summary>
        public static void SetLowQuality()
        {
            Application.targetFrameRate = 30;
            QualitySettings.SetQualityLevel(0, true);
            Debug.Log("[MobilePerformanceManager] Calidad baja activada");
        }

        /// <summary>
        /// Establecer calidad alta para dispositivos potentes
        /// </summary>
        public static void SetHighQuality()
        {
            Application.targetFrameRate = 60;
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, true);
            Debug.Log("[MobilePerformanceManager] Calidad alta activada");
        }
    }
}
