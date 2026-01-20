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

        [SerializeField] private bool yaUsado = false;

        private void Awake()
        {
            // Debug.Log($"[DialogueZone] Awake - {gameObject.name} - InstanceID: {GetInstanceID()}");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Solo reacciona al Player
            if (!other.CompareTag("Player")) return;

            // Debug.Log($"[DialogueZone] {gameObject.name} (ID:{GetInstanceID()}) - yaUsado={yaUsado}, soloUnaVez={soloUnaVez}");

            // Si ya se usó y es de una sola vez, no hacer nada
            if (soloUnaVez && yaUsado)
            {
                // Debug.Log($"[DialogueZone] {gameObject.name} - BLOQUEADO (ya usado)");
                return;
            }

            // Si ya hay un diálogo activo, no interrumpir
            if (dialogueRunner.IsDialogueRunning) return;

            // Marcar como usado ANTES de iniciar el diálogo
            yaUsado = true;
            // Debug.Log($"[DialogueZone] {gameObject.name} - yaUsado cambiado a TRUE, iniciando {nodoDialogo}");

            dialogueRunner.StartDialogue(nodoDialogo);
        }
    }
}
