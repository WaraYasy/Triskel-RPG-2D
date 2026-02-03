using UnityEngine;
using Yarn.Unity;

namespace Triskel.Dialogue
{
    /// <summary>
    /// Controla cuándo un DialogueRunner debe iniciar automáticamente.
    /// Útil para diálogos que solo deben verse una vez (ej: intro del Hub).
    /// </summary>
    public class DialogueAutoStarter : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("El DialogueRunner que queremos controlar")]
        [SerializeField] private DialogueRunner dialogueRunner;

        [Tooltip("Nodo de diálogo a iniciar (ej: Hub1)")]
        [SerializeField] private string startNode = "Hub1";

        [Header("Opciones")]
        [Tooltip("Solo ejecutar el diálogo la primera vez que se entra")]
        [SerializeField] private bool soloUnaVez = true;

        [Tooltip("Congelar al jugador durante el diálogo")]
        [SerializeField] private bool freezePlayer = true;

        [Tooltip("Clave única para guardar si ya se vio (ej: Hub_IntroVisto)")]
        [SerializeField] private string saveKey = "Hub_IntroVisto";

        [Header("Eventos")]
        public UnityEngine.Events.UnityEvent OnDialogueEnd;

        private PlayerController cachedPlayer;

        private void Start()
        {
            if (dialogueRunner == null)
            {
                dialogueRunner = FindFirstObjectByType<DialogueRunner>();
                if (dialogueRunner == null)
                {
                    Debug.LogError("[DialogueAutoStarter] No se encontró DialogueRunner.");
                    return;
                }
            }

            // Verificar si ya se vio
            if (soloUnaVez && PlayerPrefs.GetInt(saveKey, 0) == 1)
            {
                Debug.Log($"[DialogueAutoStarter] Diálogo '{startNode}' ya fue visto. Saltando.");
                return;
            }

            // Desactivar el autoStart del DialogueRunner para controlarlo manualmente
            // (esto evita conflictos)
            dialogueRunner.autoStart = false;

            // Iniciar el diálogo
            IniciarDialogo();
        }

        private void IniciarDialogo()
        {
            if (dialogueRunner.IsDialogueRunning)
            {
                Debug.LogWarning("[DialogueAutoStarter] Ya hay un diálogo en ejecución.");
                return;
            }

            // Congelar al jugador si está configurado
            if (freezePlayer)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    cachedPlayer = player.GetComponent<PlayerController>();
                    if (cachedPlayer != null)
                    {
                        cachedPlayer.enabled = false;

                        // Detener velocidad
                        var rb = player.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.linearVelocity = Vector2.zero;
                        }
                    }
                }
            }

            // Suscribirse al evento de finalización
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);

            // Iniciar el diálogo
            dialogueRunner.StartDialogue(startNode);
            Debug.Log($"[DialogueAutoStarter] Diálogo '{startNode}' iniciado.");
        }

        private void OnDialogueComplete()
        {
            // Marcar como visto
            if (soloUnaVez)
            {
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
                Debug.Log($"[DialogueAutoStarter] Diálogo '{startNode}' marcado como visto.");
            }

            // Descongelar al jugador
            if (cachedPlayer != null)
            {
                cachedPlayer.enabled = true;
                cachedPlayer = null;
            }

            // Limpiar el listener
            dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);

            // Disparar evento personalizado
            OnDialogueEnd?.Invoke();
        }

        /// <summary>
        /// Resetea el estado de visto (útil para testing o New Game).
        /// </summary>
        [ContextMenu("Reset Estado Visto")]
        public void ResetVisto()
        {
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
            Debug.Log($"[DialogueAutoStarter] Estado de '{saveKey}' reseteado.");
        }
    }
}
