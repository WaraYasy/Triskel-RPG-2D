using UnityEngine;
using Triskel.Core;

namespace Triskel.Player
{
    /// <summary>
    /// Script para objetos coleccionables en el mundo del juego.
    /// Detecta cuando el jugador interactúa y añade el item al inventario.
    ///
    /// USO: Adjunta este script a GameObjects en la escena que sean recogibles.
    ///
    /// CONFIGURACIÓN:
    /// 1. Asigna el CollectibleItem (asset ScriptableObject)
    /// 2. Configura el método de recolección (trigger o tecla)
    /// 3. Opcional: Efectos visuales/audio al recoger
    /// </summary>
    [RequireComponent(typeof(Collider2D))] // O Collider si es 3D
    public class CollectibleObject : MonoBehaviour
    {
        [Header("Configuración del Item")]
        [Tooltip("Arrastra aquí el asset del item (Lavender.asset, Lily.asset, etc.)")]
        [SerializeField] private CollectibleItem itemData;

        [Header("Método de Recolección")]
        [Tooltip("Si TRUE, se recoge al tocar. Si FALSE, requiere presionar tecla.")]
        [SerializeField] private bool autoCollect = true;

        [Tooltip("Tecla para recoger si autoCollect = false (ejemplo: E, F, Space)")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("Efectos (Opcional)")]
        [Tooltip("Partículas al recoger (opcional)")]
        [SerializeField] private GameObject pickupParticles;

        [Tooltip("Sonido al recoger (opcional)")]
        [SerializeField] private AudioClip pickupSound;

        [Tooltip("Tiempo de espera antes de destruir el objeto (para efectos)")]
        [SerializeField] private float destroyDelay = 0.5f;

        // Estado interno
        private bool isPlayerNearby = false;
        private bool wasCollected = false;

        #region Unity Lifecycle

        private void OnValidate()
        {
            // Auto-configurar collider como trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.Log($"[CollectibleObject] Collider de '{gameObject.name}' configurado como Trigger.");
            }
        }

        private void Update()
        {
            // Si requiere tecla y el jugador está cerca
            if (!autoCollect && isPlayerNearby && !wasCollected)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    CollectItem();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Detectar si es el jugador (ajusta el tag según tu proyecto)
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;

                if (autoCollect && !wasCollected)
                {
                    CollectItem();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = false;
            }
        }

        #endregion

        #region Collection Logic

        /// <summary>
        /// Lógica principal de recolección del item.
        /// </summary>
        private void CollectItem()
        {
            if (wasCollected) return; // Evitar múltiples recolecciones
            if (itemData == null)
            {
                Debug.LogError($"[CollectibleObject] '{gameObject.name}' no tiene itemData asignado.");
                return;
            }

            // Verificar que InventoryData existe
            if (InventoryData.Instance == null)
            {
                Debug.LogError("[CollectibleObject] InventoryData no está inicializado en la escena.");
                return;
            }

            // Intentar añadir al inventario
            bool success = InventoryData.Instance.AddItem(itemData);

            if (success)
            {
                wasCollected = true;

                Debug.Log($"[CollectibleObject] ✓ Item recogido: {itemData.displayName}");

                // Ejecutar efectos
                PlayPickupEffects();

                // Guardar inventario automáticamente
                SaveInventory();

                // Destruir objeto después de un delay (para que se vean los efectos)
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                Debug.LogWarning($"[CollectibleObject] No se pudo añadir '{itemData.displayName}' al inventario (lleno o duplicado).");
                // Opcional: Mostrar mensaje al jugador "Inventario lleno"
            }
        }

        /// <summary>
        /// Guarda el inventario después de recoger un item.
        /// </summary>
        private void SaveInventory()
        {
            if (InventoryPersistence.Instance != null)
            {
                InventoryPersistence.Instance.SaveInventory();
            }
            else
            {
                Debug.LogWarning("[CollectibleObject] InventoryPersistence no está inicializado. No se guardó automáticamente.");
            }
        }

        #endregion

        #region Effects

        /// <summary>
        /// Reproduce efectos visuales y de audio al recoger.
        /// </summary>
        private void PlayPickupEffects()
        {
            // Partículas
            if (pickupParticles != null)
            {
                Instantiate(pickupParticles, transform.position, Quaternion.identity);
            }

            // Sonido
            if (pickupSound != null)
            {
                // Usar AudioSource temporal para que el sonido se complete
                GameObject tempAudio = new GameObject("PickupSound");
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = pickupSound;
                audioSource.Play();
                Destroy(tempAudio, pickupSound.length);
            }

            // Opcional: Desactivar visual del objeto inmediatamente
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }

        #endregion
    }
}
