using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// BossManager - Gestor del combate final
/// Versión 2.0 - Timer + fases progresivas de dificultad
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("Configuración del Combate")]
    [SerializeField] private float combatDuration = 60f; // 60 segundos
    [SerializeField] private BulletPattern bulletPattern;
    
    [Header("Fases de Dificultad")]
    [SerializeField] private float phase1Duration = 20f; // Primeros 20s
    [SerializeField] private float phase2Duration = 20f; // Siguientes 20s
    // Fase 3 es el resto del tiempo
    
    [Header("Configuración por Fase")]
    [SerializeField] private float phase1FireRate = 1.5f;
    [SerializeField] private float phase2FireRate = 0.8f;
    [SerializeField] private float phase3FireRate = 0.4f; // ¡MUY RÁPIDO!
    
    [SerializeField] private float phase1Speed = 1f;
    [SerializeField] private float phase2Speed = 1.3f;
    [SerializeField] private float phase3Speed = 1.8f;
    
    [Header("Moral Integration")]
    [SerializeField] private float highMoralMultiplier = 0.7f;  // Más lento si moral > 0
    [SerializeField] private float lowMoralMultiplier = 1.5f;   // Más rápido si moral < 0
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Estado del combate
    private float timeRemaining;
    private bool combatActive = false;
    private int currentPhase = 1;
    
    // Eventos
    public UnityEvent OnCombatStart = new UnityEvent();
    public UnityEvent OnCombatEnd = new UnityEvent();
    public UnityEvent OnVictory = new UnityEvent();
    public UnityEvent<int> OnPhaseChange = new UnityEvent<int>();
    
    // Propiedades públicas
    public float TimeRemaining => timeRemaining;
    public float CombatProgress => 1f - (timeRemaining / combatDuration);
    public bool IsActive => combatActive;
    public int CurrentPhase => currentPhase;

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
        
        // Cambio de fases
        UpdatePhase();
        
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
        currentPhase = 1;
        
        // Configurar fase inicial
        SetPhase(1);
        
        OnCombatStart?.Invoke();
        
        if (showDebugLogs)
        {
            Debug.Log("⚔️ ¡COMBATE INICIADO! Sobrevive " + combatDuration + " segundos");
        }
    }
    
    private void UpdatePhase()
    {
        float timeElapsed = combatDuration - timeRemaining;
        int newPhase = 1;
        
        if (timeElapsed > phase1Duration + phase2Duration)
        {
            newPhase = 3; // FASE FINAL
        }
        else if (timeElapsed > phase1Duration)
        {
            newPhase = 2; // FASE INTERMEDIA
        }
        
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            SetPhase(currentPhase);
            OnPhaseChange?.Invoke(currentPhase);
        }
    }
    
    private void SetPhase(int phase)
    {
        if (bulletPattern == null) return;
        
        switch (phase)
        {
            case 1:
                bulletPattern.SetFireRate(phase1FireRate);
                bulletPattern.SetSpeedMultiplier(phase1Speed);
                bulletPattern.SetPhase(1);
                
                if (showDebugLogs)
                {
                    Debug.Log("⚡ FASE 1 - Calentamiento");
                }
                break;
            
            case 2:
                bulletPattern.SetFireRate(phase2FireRate);
                bulletPattern.SetSpeedMultiplier(phase2Speed);
                bulletPattern.SetPhase(2);
                
                if (showDebugLogs)
                {
                    Debug.Log("🔥 FASE 2 - Se pone intenso...");
                }
                break;
            
            case 3:
                bulletPattern.SetFireRate(phase3FireRate);
                bulletPattern.SetSpeedMultiplier(phase3Speed);
                bulletPattern.SetPhase(3);
                
                if (showDebugLogs)
                {
                    Debug.Log("💀 FASE 3 - ¡INFIERNO BULLET HELL!");
                }
                break;
        }
        
        // Ajustar por moral
        AdjustDifficultyByMoral();
    }
    
    private void EndCombat(bool victory)
    {
        combatActive = false;
        
        if (victory)
        {
            OnVictory?.Invoke();
            
            if (showDebugLogs)
            {
                Debug.Log("🎉 ¡VICTORIA! Has sobrevivido al infierno");
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
        
        // Ajustar velocidad según moral
        if (moral > 0)
        {
            bulletPattern.SetMoralMultiplier(highMoralMultiplier);
            if (showDebugLogs)
            {
                Debug.Log("✨ Moral > 0: Más fácil");
            }
        }
        else if (moral < 0)
        {
            bulletPattern.SetMoralMultiplier(lowMoralMultiplier);
            if (showDebugLogs)
            {
                Debug.Log("💀 Moral < 0: Más difícil");
            }
        }
        else
        {
            bulletPattern.SetMoralMultiplier(1f);
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
    [ContextMenu("Debug: Saltar a Fase 2")]
    private void DebugPhase2()
    {
        timeRemaining = combatDuration - phase1Duration - 1f;
    }
    
    [ContextMenu("Debug: Saltar a Fase 3")]
    private void DebugPhase3()
    {
        timeRemaining = combatDuration - phase1Duration - phase2Duration - 1f;
    }
    
    [ContextMenu("Debug: Victoria Instantánea")]
    private void DebugVictory()
    {
        timeRemaining = 0;
    }
}

