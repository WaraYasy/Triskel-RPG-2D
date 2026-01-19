using UnityEngine;

/// <summary>
/// SpawnPoint - Define un punto de aparición del jugador al entrar en una escena.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    [Header("Ajustes")]
    [Tooltip("Este ID debe coincidir con el 'Target Spawn ID' del LevelExit que te mandó aquí.")]
    [SerializeField] private string spawnID;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[SpawnPoint] No se encontró el GameManager. Asegúrate de que exista uno en la escena.");
            return;
        }

        string lastExit = GameManager.Instance.LastExitUsed;
        Debug.Log($"[SpawnPoint] Comprobando punto: '{spawnID}'. Última salida en GameManager: '{lastExit}'");

        // Verificar si este es el punto de aparición que el GameManager tiene guardado
        if (!string.IsNullOrEmpty(lastExit) && lastExit == spawnID)
        {
            MovePlayerToPoint();
        }
    }

    private void MovePlayerToPoint()
    {
        // Buscar al jugador
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Teletransportar al jugador a la posición de este objeto
            player.transform.position = transform.position;

            // Si tiene Rigidbody2D, hay que mover también la posición de las físicas
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = transform.position;
                rb.linearVelocity = Vector2.zero; // Evitar que entre con inercia
            }

            Debug.Log($"[SpawnPoint] ¡ÉXITO! Jugador movido al punto: {spawnID}");
        }
        else
        {
            Debug.LogError($"[SpawnPoint] ¡ERROR! Se encontró el ID '{spawnID}' pero no hay ningún objeto con el Tag 'Player' en la escena.");
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujo visual en el editor para ver dónde están los puntos
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.5f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f, "Spawn: " + spawnID);
        #endif
    }
}
