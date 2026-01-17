using UnityEngine;
using Yarn.Unity;

namespace Triskel.Dialogue
{
    /// <summary>
    /// Zona que inicia un diálogo de YarnSpinner al entrar el jugador.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DialogueZone : MonoBehaviour
    {
        [Header("Diálogo")]
        [Tooltip("Nombre del nodo en el .yarn (ej: Nivel1_Intro)")]
        public string nodoDialogo;

        [Tooltip("Arrastra aquí el DialogueRunner de la escena")]
        public DialogueRunner dialogueRunner;

        [Header("Opciones")]
        public bool soloUnaVez = true;

        private bool yaUsado = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Solo reacciona al Player
            if (!other.CompareTag("Player")) return;

            // Si ya se usó y es de una sola vez, no hacer nada
            if (soloUnaVez && yaUsado) return;

            // Si ya hay un diálogo activo, no interrumpir
            if (dialogueRunner.IsDialogueRunning) return;

            // Iniciar el diálogo
            yaUsado = true;
            dialogueRunner.StartDialogue(nodoDialogo);
        }
    }
}
