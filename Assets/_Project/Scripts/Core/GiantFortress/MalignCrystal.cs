using UnityEngine;
using UnityEngine.Events;

namespace Triskel.GiantFortress
{
    /// <summary>
    /// MalignCrystal - Roca que revela su verdadera forma (cristal maligno) 
    /// cuando la luz del Lirio esta activa cerca.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class MalignCrystal : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite rockSprite;
        [SerializeField] private Sprite crystalSprite;
        
        [Header("Deteccion del Lirio")]
        [SerializeField] private float detectionRadius = 5f;
        
        [Header("Feedback Visual")]
        [SerializeField] private ParticleSystem revealParticles;
        [SerializeField] private GameObject glowEffect;
        [SerializeField] private Color revealedGlowColor = new Color(1f, 0.3f, 0.8f, 0.5f);
        
        [Header("Eventos")]
        public UnityEvent OnRevealed;
        public UnityEvent OnCrystalDestroyed;
        
        private SpriteRenderer spriteRenderer;
        private bool isRevealed = false;
        private bool isDestroyed = false;
        private PlayerLight cachedPlayerLight;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (rockSprite != null)
            {
                spriteRenderer.sprite = rockSprite;
            }
            
            if (glowEffect != null) glowEffect.SetActive(false);
            if (revealParticles != null) revealParticles.Stop();
        }
        
        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                cachedPlayerLight = player.GetComponent<PlayerLight>();
            }
        }
        
        private void Update()
        {
            if (isDestroyed) return;
            CheckLirioProximity();
        }
        
        private void CheckLirioProximity()
        {
            if (cachedPlayerLight == null) return;
            
            float distance = Vector2.Distance(transform.position, cachedPlayerLight.transform.position);
            bool isPlayerNear = distance <= detectionRadius;
            bool lirioActive = cachedPlayerLight.IsAbilityActive();
            
            if (isPlayerNear && lirioActive)
            {
                RevealCrystal();
            }
            else
            {
                HideCrystal();
            }
        }
        
        private void RevealCrystal()
        {
            if (isRevealed) return;
            isRevealed = true;
            
            if (crystalSprite != null)
            {
                spriteRenderer.sprite = crystalSprite;
            }
            
            if (glowEffect != null)
            {
                glowEffect.SetActive(true);
                var glowRenderer = glowEffect.GetComponent<SpriteRenderer>();
                if (glowRenderer != null)
                {
                    glowRenderer.color = revealedGlowColor;
                }
            }
            
            if (revealParticles != null)
            {
                revealParticles.Play();
            }
            
            OnRevealed?.Invoke();
            Debug.Log($"Cristal Maligno revelado en {gameObject.name}");
        }
        
        private void HideCrystal()
        {
            if (!isRevealed) return;
            isRevealed = false;
            
            if (rockSprite != null)
            {
                spriteRenderer.sprite = rockSprite;
            }
            
            if (glowEffect != null)
            {
                glowEffect.SetActive(false);
            }
        }
        
        public void OnAxeHit()
        {
            if (isDestroyed) return;
            
            if (!isRevealed)
            {
                Debug.Log("Golpeaste una roca... parece que no pasa nada.");
                return;
            }
            
            DestroyCrystal();
        }
        
        private void DestroyCrystal()
        {
            isDestroyed = true;
            Debug.Log($"Cristal Maligno destruido: {gameObject.name}");
            
            OnCrystalDestroyed?.Invoke();
            Destroy(gameObject, 0.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
        
        public bool IsRevealed() => isRevealed;
        public bool IsDestroyed() => isDestroyed;
    }
}
