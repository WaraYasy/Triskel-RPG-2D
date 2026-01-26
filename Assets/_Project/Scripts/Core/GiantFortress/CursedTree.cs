using UnityEngine;
using UnityEngine.Events;

namespace Triskel.GiantFortress
{
    /// <summary>
    /// CursedTree - Arbol maldito que bloquea el camino.
    /// Se puede: A) Talar directamente (malo) o B) Sanar destruyendo cristales (bueno)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class CursedTree : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite cursedSprite;
        [SerializeField] private Sprite healedSprite;
        [SerializeField] private Sprite deadSprite;
        
        [Header("Cristales Necesarios")]
        [SerializeField] private int crystalsToHeal = 2;
        
        [Header("Feedback Visual")]
        [SerializeField] private ParticleSystem healingParticles;
        [SerializeField] private ParticleSystem deathParticles;
        [SerializeField] private GameObject corruptionAura;
        [SerializeField] private Color healedColor = Color.green;
        [SerializeField] private Color deadColor = new Color(0.3f, 0.2f, 0.1f);
        
        [Header("Dano por Hacha (Camino Malo)")]
        [SerializeField] private int hitsToChop = 3;
        private int currentHits = 0;
        
        [Header("Eventos")]
        public UnityEvent OnHealed;
        public UnityEvent OnChopped;
        
        private SpriteRenderer spriteRenderer;
        private int crystalsDestroyed = 0;
        private bool isResolved = false;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (cursedSprite != null)
            {
                spriteRenderer.sprite = cursedSprite;
            }
        }
        
        /// <summary>
        /// Llamar desde el UnityEvent de MalignCrystal.OnCrystalDestroyed
        /// </summary>
        public void OnCrystalDestroyed()
        {
            if (isResolved) return;
            
            crystalsDestroyed++;
            Debug.Log($"{name}: Cristal destruido ({crystalsDestroyed}/{crystalsToHeal})");
            
            UpdatePartialHealing();
            
            if (crystalsDestroyed >= crystalsToHeal)
            {
                HealTree();
            }
        }
        
        public void OnAxeHit()
        {
            if (isResolved) return;
            
            currentHits++;
            Debug.Log($"Golpe al arbol ({currentHits}/{hitsToChop})");
            
            StartCoroutine(ShakeEffect());
            
            if (currentHits >= hitsToChop)
            {
                ChopTree();
            }
        }
        
        private void ChopTree()
        {
            if (isResolved) return;
            isResolved = true;
            
            Debug.Log($"{name} ha sido TALADO! (Camino malo)");
            
            if (deadSprite != null)
            {
                spriteRenderer.sprite = deadSprite;
                spriteRenderer.color = deadColor;
            }
            
            if (corruptionAura != null)
            {
                corruptionAura.SetActive(false);
            }
            
            if (deathParticles != null)
            {
                deathParticles.Play();
            }
            
            OnChopped?.Invoke();
            GetComponent<Collider2D>().enabled = false;
        }
        
        private void HealTree()
        {
            if (isResolved) return;
            isResolved = true;
            
            Debug.Log($"{name} ha sido SANADO! (Camino bueno)");
            
            if (healedSprite != null)
            {
                spriteRenderer.sprite = healedSprite;
                spriteRenderer.color = healedColor;
            }
            
            if (corruptionAura != null)
            {
                corruptionAura.SetActive(false);
            }
            
            if (healingParticles != null)
            {
                healingParticles.Play();
            }
            
            OnHealed?.Invoke();
            GetComponent<Collider2D>().enabled = false;
        }
        
        private void UpdatePartialHealing()
        {
            float progress = (float)crystalsDestroyed / crystalsToHeal;
            spriteRenderer.color = Color.Lerp(Color.white, healedColor, progress * 0.5f);
            
            if (corruptionAura != null)
            {
                var auraRenderer = corruptionAura.GetComponent<SpriteRenderer>();
                if (auraRenderer != null)
                {
                    Color c = auraRenderer.color;
                    c.a = 1f - progress;
                    auraRenderer.color = c;
                }
            }
        }
        
        private System.Collections.IEnumerator ShakeEffect()
        {
            Vector3 originalPos = transform.position;
            float shakeDuration = 0.2f;
            float shakeMagnitude = 0.1f;
            
            float elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                float x = Random.Range(-1f, 1f) * shakeMagnitude;
                float y = Random.Range(-1f, 1f) * shakeMagnitude;
                transform.position = originalPos + new Vector3(x, y, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.position = originalPos;
        }
        
        public bool IsResolved() => isResolved;
    }
}
