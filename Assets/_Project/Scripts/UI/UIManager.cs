using UnityEngine;

namespace Triskel.UI
{
    /// <summary>
    /// Manager que hace persistir la UI entre escenas.
    /// Se coloca en el GameObject raíz de la UI.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Referencias (se asignan automáticamente)")]
        [SerializeField] private PauseController pauseController;
        [SerializeField] private SettingsController settingsController;

        // Propiedades públicas para acceder a los controladores
        public PauseController PauseController => pauseController;
        public SettingsController SettingsController => settingsController;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeReferences();
                Debug.Log("[UIManager] UI inicializada y persistente");
            }
            else
            {
                // Ya existe una UI, destruir esta duplicada
                Debug.Log("[UIManager] UI duplicada detectada, destruyendo...");
                Destroy(gameObject);
            }
        }

        private void InitializeReferences()
        {
            // Buscar controladores si no están asignados
            if (pauseController == null)
                pauseController = GetComponentInChildren<PauseController>(true);

            if (settingsController == null)
                settingsController = GetComponentInChildren<SettingsController>(true);

            // Validación
            if (pauseController == null)
                Debug.LogWarning("[UIManager] PauseController no encontrado");
            if (settingsController == null)
                Debug.LogWarning("[UIManager] SettingsController no encontrado");
        }
    }
}
