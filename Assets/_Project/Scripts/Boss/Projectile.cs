using UnityEngine;

/// <summary>
/// Projectile - Proyectil del boss
/// Versión 1.0 - Movimiento simple en dirección
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 10f;
    
    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Auto-destruirse después de X segundos
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 moveDirection, float projectileSpeed)
    {
        direction = moveDirection.normalized;
        speed = projectileSpeed;
        
        // Aplicar velocidad
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si toca al player
        if (collision.CompareTag("Player"))
        {
            // TODO: Hacer daño al player
            Debug.Log("💥 Player golpeado!");
            Destroy(gameObject);
        }
        
        // Si toca una pared (descomentar si creas paredes)
        // if (collision.CompareTag("Wall"))
        // {
        //     Destroy(gameObject);
        // }
    }
}
