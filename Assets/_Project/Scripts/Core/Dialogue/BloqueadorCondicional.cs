using UnityEngine;
using Triskel.Dialogue;

namespace Triskel.Dialogue
{
    /// <summary>
    /// Objeto que bloquea físicamente el paso del jugador hasta que se complete una condición.
    /// Se combina con un DialogueZone que se repite hasta completar la misma condición.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BloqueadorCondicional : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("DialogueZone asociado (con 'Repetir Hasta Condicion' activado)")]
        [SerializeField] private DialogueZone dialogoAsociado;

        [Header("Configuración")]
        [Tooltip("Si está marcado, el objeto se destruye al completar. Si no, solo se desactiva.")]
        [SerializeField] private bool destruirAlCompletar = false;

        [Tooltip("Efecto visual/sonoro al desbloquear (opcional)")]
        [SerializeField] private GameObject efectoDesbloqueo;

        private bool condicionCompletada = false;
        private Collider2D bloqueadorCollider;

        private void Awake()
        {
            bloqueadorCollider = GetComponent<Collider2D>();

            // Asegurar que el collider NO es trigger (para que bloquee físicamente)
            if (bloqueadorCollider.isTrigger)
            {
                Debug.LogWarning($"[BloqueadorCondicional] El Collider2D de '{gameObject.name}' está marcado como Trigger. Se cambiará a NO trigger para bloquear el paso.", this);
                bloqueadorCollider.isTrigger = false;
            }
        }

        /// <summary>
        /// Completa la condición y desbloquea el paso.
        /// Llama a este método desde tu código cuando se cumpla la condición.
        /// </summary>
        public void CompletarCondicion()
        {
            if (condicionCompletada) return; // Ya completado

            condicionCompletada = true;
            Debug.Log($"[BloqueadorCondicional] Condición completada. Desbloqueando '{gameObject.name}'");

            // Completar también el diálogo asociado
            if (dialogoAsociado != null)
            {
                dialogoAsociado.CompletarCondicion();
            }

            // Reproducir efecto visual si existe
            if (efectoDesbloqueo != null)
            {
                Instantiate(efectoDesbloqueo, transform.position, Quaternion.identity);
            }

            // Destruir o desactivar el bloqueador
            if (destruirAlCompletar)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Resetea el bloqueador. Útil para testing o reiniciar niveles.
        /// </summary>
        public void ResetearCondicion()
        {
            condicionCompletada = false;
            gameObject.SetActive(true);

            if (dialogoAsociado != null)
            {
                dialogoAsociado.ResetearCondicion();
            }

            Debug.Log($"[BloqueadorCondicional] Bloqueador '{gameObject.name}' reseteado.");
        }

        // Métodos públicos para verificar estado (útiles para debugging)
        public bool EstaCompletado() => condicionCompletada;
    }
}
