using UnityEngine;

/// <summary>
/// SafeZone - Zona segura donde las sombras no pueden entrar.
/// El jugador está automáticamente oculto al entrar.
/// Mecánica de sigilo para el Cuadrante 3.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SafeZone : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private SpriteRenderer zoneVisual;
    [SerializeField] private Color safeColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
    [SerializeField] private bool showVisualInGame = false;
    
    [Header("Indicador")]
    [SerializeField] private GameObject safeIndicator; // Icono de "seguro" cuando el jugador está dentro
    
    private bool playerInside = false;
    private PlayerStealth playerStealth;

    private void Awake()
    {
        // Asegurar que el collider es trigger
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        
        if (zoneVisual != null && !showVisualInGame)
        {
            zoneVisual.enabled = false;
        }
        
        if (safeIndicator != null) safeIndicator.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerStealth = other.GetComponent<PlayerStealth>();
            
            if (playerStealth != null)
            {
                playerStealth.SetHidden(true, null);
            }
            
            if (safeIndicator != null) safeIndicator.SetActive(true);
            
            Debug.Log("🛡️ Jugador en zona segura");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            
            if (playerStealth != null)
            {
                playerStealth.SetHidden(false, null);
            }
            
            if (safeIndicator != null) safeIndicator.SetActive(false);
            
            playerStealth = null;
            
            Debug.Log("⚠️ Jugador sale de zona segura");
        }
    }

    public bool IsPlayerInside() => playerInside;

    private void OnDrawGizmos()
    {
        // Visualizar la zona segura en el editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = safeColor;
            
            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            else if (col is PolygonCollider2D poly)
            {
                // Dibujar polígono
                for (int i = 0; i < poly.points.Length; i++)
                {
                    Vector2 p1 = transform.TransformPoint(poly.points[i]);
                    Vector2 p2 = transform.TransformPoint(poly.points[(i + 1) % poly.points.Length]);
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
    }
}
