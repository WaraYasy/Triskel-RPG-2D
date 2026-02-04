// =======================================================================================
// Triskel RPG 2D - Controls Display Trigger
// =======================================================================================
// Autor: Mandrágora - Wara Pacheco
// Descripción: Trigger que permite al jugador abrir la UI de controles al interactuar
//              con un objeto en el mundo (por ejemplo, un cartel de controles).
// =======================================================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace Triskel.UI
{
    /// <summary>
    /// Trigger para mostrar la UI de controles cuando el jugador interactúa.
    /// </summary>
    public class ControlsDisplayTrigger : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private ControlsDisplayController controlsUI;

        [Header("Configuración")]
        [SerializeField] private bool showPrompt = true;

        private bool playerInRange = false;
        private GameObject promptUI; // Opcional: UI que muestra "Presiona E"

        private void Update()
        {
            // Si el jugador está en rango y presiona E (usando nuevo Input System)
            if (playerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                OpenControls();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                Debug.Log("[ControlsTrigger] Jugador en rango. Presiona E para ver controles.");

                // Aquí puedes mostrar un prompt visual si lo deseas
                if (showPrompt)
                {
                    ShowPrompt();
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // Mantener el estado mientras el jugador esté dentro
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                Debug.Log("[ControlsTrigger] Jugador fuera de rango.");

                if (showPrompt)
                {
                    HidePrompt();
                }
            }
        }

        private void OpenControls()
        {
            if (controlsUI != null)
            {
                controlsUI.Show();
            }
            else
            {
                Debug.LogWarning("[ControlsTrigger] ControlsDisplayController no asignado");
            }
        }

        private void ShowPrompt()
        {
            // TODO: Mostrar UI de "Presiona E" sobre el objeto
            // Puedes añadir un WorldSpace Canvas con texto aquí
        }

        private void HidePrompt()
        {
            // TODO: Ocultar UI de "Presiona E"
        }

        private void OnDrawGizmos()
        {
            // Dibujar el área del trigger en el editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
