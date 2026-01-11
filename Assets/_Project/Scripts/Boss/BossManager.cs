using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// BossManager - Gestor del combate final
/// Versión 1.0 - Timer de supervivencia + patrones de disparo
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("Configuración del Combate")]
    [SerializeField] private float combatDuration = 60f; // 60 segundos
    [SerializeField] private BulletPattern bulletPattern;
    
    [Header("Moral Integration")]
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float highMoralSpeed = 0.7f;  // Más lento si moral > 0
    [SerializeField] private float lowMoralSpeed = 1.5f;   // Más rápido si moral < 0
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Estado del combate
    private float timeRemaining;
    private bool combatActive = false;
    
    // Eventos
    public UnityEvent OnCombatStart = new UnityEvent();
    public UnityEvent OnCombatEnd = new UnityEvent();
    public UnityEvent OnVictory = new UnityEvent();
    
    // Propiedades públicas
    public float TimeRemaining => timeRemaining;
    public float CombatProgress => 1f - (timeRemaining / combatDuration);
    public bool IsActive => combatActive;

    private void Start()
    {
        timeRemaining = combatDuration;
        StartCombat();
    }

    private void Update()
    {
        if (!combatActive) return;
        
        // Countdown del timer
        timeRemaining -= Time.deltaTime;
        
        // Victoria si sobrevive el tiempo
        if (timeRemaining <= 0)
        {
            EndCombat(true);
        }
    }
    
    public void StartCombat()
    {
        combatActive = true;
        timeRemaining = combatDuration;
        
        // Ajustar velocidad según moral
        AdjustDifficultyByMoral();
        
        OnCombatStart?.Invoke();
        
        if (showDebugLogs)
        {
            Debug.Log("⚔️ ¡COMBATE INICIADO! Sobrevive " + combatDuration + " segundos");
        }
    }
    
    private void EndCombat(bool victory)
    {
        combatActive = false;
        
        if (victory)
        {
            OnVictory?.Invoke();
            
            if (showDebugLogs)
            {
                Debug.Log("🎉 ¡VICTORIA! Has sobrevivido");
            }
        }
        
        OnCombatEnd?.Invoke();
    }
    
    private void AdjustDifficultyByMoral()
    {
        if (bulletPattern == null) return;
        
        // Obtener moral del GameManager
        int moral = 0;
        if (GameManager.Instance != null)
        {
            // TODO: Descomentar cuando GameManager tenga MoralScore
            // moral = GameManager.Instance.MoralScore;
        }
        
        // Ajustar velocidad de proyectiles según moral
        if (moral > 0)
        {
            // Moral positiva = Más fácil
            bulletPattern.SetProjectileSpeed(highMoralSpeed);
            
            if (showDebugLogs)
            {
                Debug.Log("✨ Moral > 0: Proyectiles más lentos");
            }
        }
        else if (moral < 0)
        {
            // Moral negativa = Más difícil
            bulletPattern.SetProjectileSpeed(lowMoralSpeed);
            
            if (showDebugLogs)
            {
                Debug.Log("💀 Moral < 0: Proyectiles más rápidos");
            }
        }
        else
        {
            // Moral neutral
            bulletPattern.SetProjectileSpeed(normalSpeed);
        }
    }
    
    // Método para terminar el combate manualmente (derrota)
    public void PlayerDefeated()
    {
        EndCombat(false);
        
        if (showDebugLogs)
        {
            Debug.Log("💀 DERROTA - El jugador ha caído");
        }
    }
    
    // Debug helpers
    [ContextMenu("Debug: Terminar Combate (Victoria)")]
    private void DebugVictory()
    {
        timeRemaining = 0;
    }
    
    [ContextMenu("Debug: Añadir 30 segundos")]
    private void DebugAddTime()
    {
        timeRemaining += 30f;
    }
}
