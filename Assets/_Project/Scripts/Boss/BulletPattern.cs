using UnityEngine;

/// <summary>
/// BulletPattern - Patrones de disparo del boss
/// Versión 2.0 - Sistema de fases con dificultad progresiva
/// </summary>
public class BulletPattern : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private GameObject dangerZonePrefab; // NUEVO: Prefab de zona de peligro
    [SerializeField] private Transform firePoint;
    
    [Header("Configuración de Patrones")]
    [SerializeField] private float baseProjectileSpeed = 5f;
    [SerializeField] private float fireRate = 1f;
    
    private float nextFireTime = 0f;
    private float speedMultiplier = 1f;
    private float moralMultiplier = 1f;
    private int currentPhase = 1;
    
    public enum PatternType
    {
        Circle,      // Disparo circular (360°)
        Line,        // Disparo en línea recta
        Spread,      // Abanico de proyectiles
        Spiral,      // Espiral rotante
        Cross,       // Cruz (+)
        Random       // Aleatorio caótico
    }
    
    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FirePatternByPhase();
            nextFireTime = Time.time + fireRate;
        }
    }
    
    // === SECUENCIAS DE ATAQUES POR FASE ===
    
    private int attackSequenceIndex = 0;
    
    private void FirePatternByPhase()
    {
        switch (currentPhase)
        {
            case 1: // FASE 1 - Secuencia simple y predecible
                FirePhase1Sequence();
                break;
            
            case 2: // FASE 2 - Secuencia más compleja
                FirePhase2Sequence();
                break;
            
            case 3: // FASE 3 - Secuencia intensa pero aprendible
                FirePhase3Sequence();
                break;
        }
        
        attackSequenceIndex++;
    }
    
    private void FirePhase1Sequence()
    {
        // Secuencia: Circle → Spread → Line → Repeat
        int attack = attackSequenceIndex % 3;
        
        switch (attack)
        {
            case 0:
                FirePattern(PatternType.Circle);
                break;
            case 1:
                FirePattern(PatternType.Spread);
                break;
            case 2:
                FirePattern(PatternType.Line);
                break;
        }
    }
    
    private void FirePhase2Sequence()
    {
        // Secuencia: Spiral → DangerZone → Circle → Cross → DangerZone → Spread → Repeat
        int attack = attackSequenceIndex % 6;
        
        switch (attack)
        {
            case 0:
                FirePattern(PatternType.Spiral);
                break;
            case 1:
                // Zona centrada debajo del boss
                Vector3 bossPos = transform.position;
                SpawnDangerZone(bossPos + Vector3.down * 3f, 2.5f);
                break;
            case 2:
                FirePattern(PatternType.Circle);
                break;
            case 3:
                FirePattern(PatternType.Cross);
                break;
            case 4:
                // Dos zonas a los lados, abajo
                Vector3 pos = transform.position;
                SpawnDangerZone(pos + new Vector3(-3f, -3f, 0), 2f);
                SpawnDangerZone(pos + new Vector3(3f, -3f, 0), 2f);
                break;
            case 5:
                FirePattern(PatternType.Spread);
                break;
        }
    }
    
    private void FirePhase3Sequence()
    {
        // Secuencia compleja pero repetible
        int attack = attackSequenceIndex % 6;
        
        switch (attack)
        {
            case 0: // Spiral + Circle
                FirePattern(PatternType.Spiral);
                FirePattern(PatternType.Circle);
                break;
            
            case 1: // Cross solo (respiro)
                FirePattern(PatternType.Cross);
                break;
            
            case 2: // Spread + Line
                FirePattern(PatternType.Spread);
                FirePattern(PatternType.Line);
                break;
            
            case 3: // Circle grande
                FireCircle(16); // Más denso
                break;
            
            case 4: // Spiral + Cross
                FirePattern(PatternType.Spiral);
                FirePattern(PatternType.Cross);
                break;
            
            case 5: // Random controlado (15 proyectiles)
                FireRandom(15);
                break;
        }
    }
    
    public void FirePattern(PatternType pattern)
    {
        switch (pattern)
        {
            case PatternType.Circle:
                FireCircle(12);
                break;
            
            case PatternType.Line:
                FireLine(Vector2.down);
                break;
            
            case PatternType.Spread:
                FireSpread(5, 60f);
                break;
            
            case PatternType.Spiral:
                FireSpiral(8);
                break;
            
            case PatternType.Cross:
                FireCross();
                break;
            
            case PatternType.Random:
                FireRandom(15);
                break;
        }
    }
    
    // === PATRONES BÁSICOS ===
    
    private void FireCircle(int count)
    {
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            SpawnProjectile(direction, GetFinalSpeed());
        }
    }
    
    private void FireLine(Vector2 direction)
    {
        SpawnProjectile(direction.normalized, GetFinalSpeed());
    }
    
    private void FireSpread(int count, float spreadAngle)
    {
        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (count - 1);
        
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
            SpawnProjectile(direction, GetFinalSpeed());
        }
    }
    
    private void FireSpiral(int count)
    {
        float angleOffset = Time.time * 50f;
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = (i * angleStep) + angleOffset;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            SpawnProjectile(direction, GetFinalSpeed());
        }
    }
    
    // === PATRONES NUEVOS (FASE 3) ===
    
    private void FireCross()
    {
        // Dispara en 4 direcciones cardinales
        SpawnProjectile(Vector2.up, GetFinalSpeed());
        SpawnProjectile(Vector2.down, GetFinalSpeed());
        SpawnProjectile(Vector2.left, GetFinalSpeed());
        SpawnProjectile(Vector2.right, GetFinalSpeed());
    }
    
    private void FireRandom(int count)
    {
        // Dispara en direcciones completamente aleatorias
        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            
            // Velocidad también aleatoria para caos máximo
            float randomSpeed = GetFinalSpeed() * Random.Range(0.7f, 1.3f);
            SpawnProjectile(direction, randomSpeed);
        }
    }
    
    // === SPAWN Y CONFIGURACIÓN ===
    
    private void SpawnProjectile(Vector2 direction, float speed)
    {
        if (projectilePrefab == null || firePoint == null) return;
        
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile projScript = proj.GetComponent<Projectile>();
        
        if (projScript != null)
        {
            projScript.Initialize(direction, speed);
        }
    }
    
    private void SpawnDangerZone(Vector3 position, float radius, float warningTime = 1.5f)
    {
        if (dangerZonePrefab == null) return;
        
        GameObject zone = Instantiate(dangerZonePrefab, position, Quaternion.identity);
        DangerZone zoneScript = zone.GetComponent<DangerZone>();
        
        if (zoneScript != null)
        {
            zoneScript.Initialize(position, radius, warningTime);
        }
    }
    
    private float GetFinalSpeed()
    {
        return baseProjectileSpeed * speedMultiplier * moralMultiplier;
    }
    
    // === MÉTODOS PÚBLICOS (Llamados por BossManager) ===
    
    public void SetFireRate(float rate)
    {
        fireRate = rate;
    }
    
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
    
    public void SetMoralMultiplier(float multiplier)
    {
        moralMultiplier = multiplier;
    }
    
    public void SetPhase(int phase)
    {
        currentPhase = phase;
    }
    
    // Método legacy para compatibilidad
    public void SetProjectileSpeed(float speedMultiplier)
    {
        this.speedMultiplier = speedMultiplier;
    }
}

