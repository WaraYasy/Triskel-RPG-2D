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
        public bool freezePlayer = true;

        [Tooltip("Marcar para diálogos del Hub que persisten entre habitaciones. Desmarcar para niveles que se resetean")]
        public bool persisteEntreEscenas = false;

        [SerializeField] private bool yaUsado = false;
        private PlayerController cachedPlayer;

        private void Start()
        {
            // Si persiste entre escenas, cargar el estado guardado
            if (persisteEntreEscenas && soloUnaVez)
            {
                string key = GetSaveKey();
                yaUsado = PlayerPrefs.GetInt(key, 0) == 1;
            }
        }

        private string GetSaveKey()
        {
            return "DialogueZone_" + gameObject.scene.name + "_" + nodoDialogo;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Solo reacciona al Player
            if (!other.CompareTag("Player")) return;

            // Si ya se usó y es de una sola vez, no hacer nada
            if (soloUnaVez && yaUsado) return;

            // Si ya hay un diálogo activo, no interrumpir
            if (dialogueRunner.IsDialogueRunning) return;

            // Marcar como usado ANTES de iniciar el diálogo
            yaUsado = true;

            // Si persiste entre escenas, guardar el estado
            if (persisteEntreEscenas && soloUnaVez)
            {
                string key = GetSaveKey();
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }

            // Congelar al jugador si la opción está activa
            if (freezePlayer)
            {
                cachedPlayer = other.GetComponent<PlayerController>();
                if (cachedPlayer != null)
                {
                    cachedPlayer.enabled = false;
                    
                    // Detener velocidad física para que no deslice
                    Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }

                    // Suscribirse para descongelarlo cuando acabe el diálogo
                    dialogueRunner.onDialogueComplete.AddListener(UnfreezePlayer);
                }
            }

            dialogueRunner.StartDialogue(nodoDialogo);
        }

        private void UnfreezePlayer()
        {
            if (cachedPlayer != null)
            {
                cachedPlayer.enabled = true;
                cachedPlayer = null;
            }
            // Limpiar el evento para que no se acumule
            dialogueRunner.onDialogueComplete.RemoveListener(UnfreezePlayer);
        }
    }
}
