using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GhostFogBarrier - Barrera de niebla fantasmal que bloquea el paso.
/// Se desactiva cuando todos los fantasmas registrados han sido liberados o destruidos.
/// </summary>
public class GhostFogBarrier : MonoBehaviour
{
    [Header("Fantasmas a Rastrear")]
    [Tooltip("Arrastra aquí todos los fantasmas que deben ser resueltos para abrir la barrera")]
    [SerializeField] private List<GameObject> ghostsToTrack = new List<GameObject>();
    
    [Header("Componentes")]
    [Tooltip("El Particle System de la niebla (opcional, se busca automáticamente)")]
    [SerializeField] private ParticleSystem fogParticles;
    [Tooltip("El Collider que bloquea el paso (opcional, se busca automáticamente)")]
    [SerializeField] private Collider2D barrierCollider;
    
    [Header("Configuración de Desvanecimiento")]
    [SerializeField] private float fadeOutDuration = 2f;
    
    [Header("Audio (Opcional)")]
    [SerializeField] private AudioClip barrierOpenSound;
    
    [Header("UI / Diálogos")]
    [Tooltip("Objeto de diálogo (ej: 'dialoguefog') que se desactivará al abrir la barrera")]
    [SerializeField] private GameObject dialogueFog;
    
    private bool isBarrierOpen = false;
    private int initialGhostCount;
    private AudioSource audioSource;
    
    private void Start()
    {
        // Buscar componentes si no están asignados
        if (fogParticles == null)
            fogParticles = GetComponentInChildren<ParticleSystem>();
        
        if (barrierCollider == null)
            barrierCollider = GetComponent<Collider2D>();
        
        audioSource = GetComponent<AudioSource>();
        
        // Limpiar lista de fantasmas nulos
        ghostsToTrack.RemoveAll(g => g == null);
        initialGhostCount = ghostsToTrack.Count;
        
        if (initialGhostCount == 0)
        {
            Debug.LogWarning("[GhostFogBarrier] ¡No hay fantasmas asignados! La barrera se abrirá inmediatamente.");
            OpenBarrier();
        }
        else
        {
            Debug.Log($"[GhostFogBarrier] Rastreando {initialGhostCount} fantasmas.");
        }
    }
    
    private void Update()
    {
        if (isBarrierOpen) return;
        
        // Comprobar si todos los fantasmas han sido destruidos/liberados
        ghostsToTrack.RemoveAll(g => g == null);
        
        if (ghostsToTrack.Count == 0)
        {
            OpenBarrier();
        }
    }
    
    private void OpenBarrier()
    {
        if (isBarrierOpen) return;
        isBarrierOpen = true;
        
        Debug.Log("🌫️ ¡Barrera de niebla disuelta! Todos los fantasmas han sido resueltos.");
        
        // Desactivar colisión inmediatamente
        if (barrierCollider != null)
        {
            barrierCollider.enabled = false;
        }
        
        // Reproducir sonido
        if (audioSource != null && barrierOpenSound != null)
        {
            audioSource.PlayOneShot(barrierOpenSound);
        }
        
        // Desactivar diálogo de pista si existe
        if (dialogueFog != null)
        {
            dialogueFog.SetActive(false);
            Debug.Log("[GhostFogBarrier] Diálogo desactivado.");
        }

        // Desvanecer partículas
        if (fogParticles != null)
        {
            StartCoroutine(FadeOutParticles());
        }
    }
    
    private System.Collections.IEnumerator FadeOutParticles()
    {
        // Detener emisión de nuevas partículas
        var emission = fogParticles.emission;
        emission.enabled = false;
        
        // Esperar a que las partículas existentes desaparezcan
        yield return new WaitForSeconds(fadeOutDuration);
        
        // Desactivar el objeto completo
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Método público para registrar fantasmas dinámicamente (si no los asignas en el Inspector)
    /// </summary>
    public void RegisterGhost(GameObject ghost)
    {
        if (ghost != null && !ghostsToTrack.Contains(ghost))
        {
            ghostsToTrack.Add(ghost);
            Debug.Log($"[GhostFogBarrier] Fantasma registrado. Total: {ghostsToTrack.Count}");
        }
    }
    
    /// <summary>
    /// Devuelve el progreso de resolución (0 = ninguno resuelto, 1 = todos resueltos)
    /// </summary>
    public float GetProgress()
    {
        if (initialGhostCount == 0) return 1f;
        int resolved = initialGhostCount - ghostsToTrack.Count;
        return (float)resolved / initialGhostCount;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualizar la barrera en el editor
        Gizmos.color = new Color(0.5f, 0f, 0.8f, 0.4f);
        
        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}
