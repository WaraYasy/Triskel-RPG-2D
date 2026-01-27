using UnityEngine;
using UnityEngine.Events;

namespace Triskel.Environment
{
    /// <summary>
    /// Árbol talable - Requiere múltiples golpes de hacha para ser talado
    /// Usado para bloquear caminos y avanzar en el nivel
    /// </summary>
    public class ChoppableTree : MonoBehaviour, Triskel.GiantFortress.IAxeDestructible
    {
        [Header("Configuración")]
        [SerializeField] private int hitsRequired = 3;
        [SerializeField] private float hitCooldown = 0.3f; // Evita múltiples golpes instantáneos
        
        [Header("Feedback Visual")]
        [SerializeField] private Sprite[] damageSprites; // Sprites para cada nivel de daño (opcional)
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeDuration = 0.15f;
        [SerializeField] private Color hitFlashColor = new Color(1f, 0.8f, 0.8f);
        
        [Header("Efectos al Talar")]
        [SerializeField] private GameObject fallEffectPrefab; // Efecto de caída (opcional)
        [SerializeField] private GameObject leavesParticles; // Partículas de hojas (opcional)
        [SerializeField] private AudioClip[] hitSounds;
        [SerializeField] private AudioClip fallSound;
        
        [Header("Eventos")]
        public UnityEvent OnHit;
        public UnityEvent OnChopped;
        
        private int currentHits = 0;
        private float lastHitTime = -999f;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private Vector3 originalPosition;
        private Color originalColor;
        private bool isChopped = false;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.5f;
            }
            
            originalPosition = transform.position;
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }
        
        /// <summary>
        /// Implementación de IAxeDestructible - Llamado cuando el hacha golpea
        /// </summary>
        public void OnAxeHit()
        {
            if (isChopped) return;
            
            // Verificar cooldown para evitar múltiples golpes
            if (Time.time - lastHitTime < hitCooldown) return;
            lastHitTime = Time.time;
            
            currentHits++;
            Debug.Log($"🌲 Árbol golpeado: {currentHits}/{hitsRequired}");
            
            // Feedback visual y sonoro
            PlayHitFeedback();
            
            // Actualizar sprite si hay sprites de daño
            UpdateDamageSprite();
            
            // Invocar evento de golpe
            OnHit?.Invoke();
            
            // Verificar si se ha talado
            if (currentHits >= hitsRequired)
            {
                ChopDown();
            }
        }
        
        private void PlayHitFeedback()
        {
            // Reproducir sonido
            if (hitSounds != null && hitSounds.Length > 0)
            {
                AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clip);
            }
            
            // Efecto de sacudida
            StartCoroutine(ShakeEffect());
            
            // Flash de color
            StartCoroutine(FlashEffect());
            
            // Partículas de hojas
            if (leavesParticles != null)
            {
                Instantiate(leavesParticles, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }
        }
        
        private System.Collections.IEnumerator ShakeEffect()
        {
            float elapsed = 0f;
            
            while (elapsed < shakeDuration)
            {
                float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
                transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = originalPosition;
        }
        
        private System.Collections.IEnumerator FlashEffect()
        {
            if (spriteRenderer == null) yield break;
            
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
        
        private void UpdateDamageSprite()
        {
            if (damageSprites == null || damageSprites.Length == 0 || spriteRenderer == null) return;
            
            // Calcular índice basado en el daño
            int damageIndex = Mathf.Clamp(currentHits - 1, 0, damageSprites.Length - 1);
            spriteRenderer.sprite = damageSprites[damageIndex];
        }
        
        private void ChopDown()
        {
            isChopped = true;
            Debug.Log("🪓 ¡Árbol talado! Camino liberado.");
            
            // Reproducir sonido de caída
            if (fallSound != null)
            {
                audioSource.PlayOneShot(fallSound);
            }
            
            // Invocar evento
            OnChopped?.Invoke();
            
            // Efecto de caída
            if (fallEffectPrefab != null)
            {
                Instantiate(fallEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Animación de caída y destrucción
            StartCoroutine(FallAnimation());
        }
        
        private System.Collections.IEnumerator FallAnimation()
        {
            // Animación simple de caída (rotar y desvanecer)
            float duration = 0.5f;
            float elapsed = 0f;
            
            Vector3 originalScale = transform.localScale;
            Quaternion originalRotation = transform.rotation;
            
            // Dirección de caída aleatoria
            float fallDirection = Random.value > 0.5f ? 1f : -1f;
            
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                
                // Rotar
                transform.rotation = Quaternion.Euler(0, 0, fallDirection * t * 90f);
                
                // Escalar ligeramente hacia abajo
                transform.localScale = originalScale * (1f - t * 0.3f);
                
                // Desvanecer
                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f - t;
                    spriteRenderer.color = c;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Desactivar el collider para liberar el camino
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = false;
            }
            
            // Esperar a que termine el sonido y luego destruir
            yield return new WaitForSeconds(0.5f);
            
            // Opción: Destruir el objeto o dejarlo como decoración
            // Destroy(gameObject);
            
            // Alternativa: Dejarlo en el suelo como decoración
            gameObject.layer = LayerMask.NameToLayer("Background");
        }
        
        /// <summary>
        /// Obtiene el progreso de tala (0-1)
        /// </summary>
        public float GetChopProgress()
        {
            return (float)currentHits / hitsRequired;
        }
        
        /// <summary>
        /// Verifica si el árbol ya fue talado
        /// </summary>
        public bool IsChopped()
        {
            return isChopped;
        }
        
        /// <summary>
        /// Reinicia el árbol (para usar con pooling o respawn)
        /// </summary>
        public void Reset()
        {
            currentHits = 0;
            isChopped = false;
            transform.position = originalPosition;
            transform.rotation = Quaternion.identity;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
                
                // Restaurar sprite original si hay sprites de daño
                if (damageSprites != null && damageSprites.Length > 0)
                {
                    // Asumimos que el primer sprite es el original
                    // En realidad deberías guardar el sprite original en Awake
                }
            }
            
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.enabled = true;
            }
        }
        
        // Visualización en el editor
        private void OnDrawGizmos()
        {
            // Mostrar área del árbol
            Gizmos.color = isChopped ? Color.gray : Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1.5f, 0f));
            
            // Mostrar progreso de daño
            if (currentHits > 0 && !isChopped)
            {
                Gizmos.color = Color.yellow;
                float progress = GetChopProgress();
                Gizmos.DrawCube(transform.position + Vector3.up * 1f, 
                    new Vector3(progress, 0.1f, 0f));
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, new Vector3(1f, 1.5f, 0f));
        }
    }
}
