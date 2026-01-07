using UnityEngine;

/// <summary>
/// Controlador básico del jugador - Movimiento en 2D
/// Versión 1.0 - Solo movimiento con WASD
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Leer input del teclado
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D o Flechas
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S o Flechas
    }

    private void FixedUpdate()
    {
        // Aplicar movimiento
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
