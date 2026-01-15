using UnityEngine;

/// <summary>
/// Script simple de NPC - Presiona E cerca del NPC para hablar
/// </summary>
public class NPC : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [Tooltip("Nombre del knot en Ink para este NPC (ej: 'npc', 'merchant')")]
    [SerializeField] private string dialogueKnotName = "npc";

    [Header("Detección del Jugador")]
    [Tooltip("Rango para detectar al jugador")]
    [SerializeField] private float interactionRange = 2f;

    [Tooltip("Tag del jugador")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange = false;
    private Transform player;

    private void Update()
    {
        // Buscar jugador cercano
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Verificar si el jugador está en rango
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            playerInRange = distance <= interactionRange;
        }

        // Si el jugador presiona E y está en rango, iniciar diálogo
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    /// <summary>
    /// Inicia el diálogo con este NPC
    /// </summary>
    private void StartDialogue()
    {
        Debug.Log($"[NPC] Iniciando diálogo: {dialogueKnotName}");
        GameManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
    }

    // Visualizar el rango en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
