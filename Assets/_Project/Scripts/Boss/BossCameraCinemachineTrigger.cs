using UnityEngine;
using Unity.Cinemachine;

namespace Triskel.Boss
{
    public class BossCameraCinemachineTrigger : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private CinemachineCamera bossVirtualCamera;
        
        [Header("Configuración")]
        [SerializeField] private int highPriority = 20;

        private void Start()
        {
            // Verificamos si el componente existe y está bien configurado
            Debug.Log($"[BossCameraTrigger] Script iniciado en {gameObject.name}. Esperando al jugador...");
            
            // Auto-configuración de físicas
            BoxCollider2D col = GetComponent<BoxCollider2D>();
            if (col == null) Debug.LogError($"[BossCameraTrigger] ¡ERROR! No hay BoxCollider2D en {gameObject.name}");
            else if (!col.isTrigger) Debug.LogWarning($"[BossCameraTrigger] {gameObject.name} tiene un collider que NO es Trigger.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"[BossCameraTrigger] COLISIÓN DETECTADA con: {other.gameObject.name} (Tag: {other.tag})");

            if (other.CompareTag("Player"))
            {
                if (bossVirtualCamera != null)
                {
                    bossVirtualCamera.Priority = highPriority;
                    Debug.Log("🎥 Cámara de Boss v3 Activada.");
                }
            }
        }
    }
}
