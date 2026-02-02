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
                // Zona centrada (ajustado para estar más cerca del boss)
                Vector3 bossPos = transform.position;
                SpawnDangerZone(bossPos + Vector3.down * 9f, 2.5f); 
                break;
            case 2:
                FirePattern(PatternType.Circle);
                break;
            case 3:
                FirePattern(PatternType.Cross);
                break;
            case 4:
                // Dos zonas a los lados (ajustado para estar más cerca)
                Vector3 pos = transform.position;
                SpawnDangerZone(pos + new Vector3(-6f, -11f, 0), 2f);
                SpawnDangerZone(pos + new Vector3(6f, -11f, 0), 2f);
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
        // Modificado: Ahora dispara en un semicírculo hacia abajo (180 grados)
        float startAngle = 180f; // Izquierda
        float endAngle = 360f;   // Derecha
        float angleStep = (endAngle - startAngle) / (count - 1);
        
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (i * angleStep);
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
        // Modificado: Espiral limitada al arco inferior
        float angleOffset = Mathf.PingPong(Time.time * 100f, 180f) - 90f; // Oscila -90 a 90
        float angleStep = 45f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = 270f + angleOffset + (i * angleStep); // Centrado en Down (270)
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            SpawnProjectile(direction, GetFinalSpeed());
        }
    }
    
    // === PATRONES NUEVOS (FASE 3) ===
    
    private void FireCross()
    {
        // Modificado: Solo direcciones hacia abajo
        SpawnProjectile(Vector2.down, GetFinalSpeed());
        SpawnProjectile(new Vector2(-1, -1).normalized, GetFinalSpeed());
        SpawnProjectile(new Vector2(1, -1).normalized, GetFinalSpeed());
    }
    
    private void FireRandom(int count)
    {
        // Modificado: Direcciones aleatorias solo hacia abajo (arco de 180)
        for (int i = 0; i < count; i++)
        {
            float randomAngle = Random.Range(180f, 360f); // Arco inferior
            Vector2 direction = Quaternion.Euler(0, 0, randomAngle) * Vector2.right;
            
            float randomSpeed = GetFinalSpeed() * Random.Range(0.7f, 1.3f);
            SpawnProjectile(direction, randomSpeed);
        }
    }
    
    // === SPAWN Y CONFIGURACIÓN ===
    
    private void SpawnProjectile(Vector2 direction, float speed)
    {
        if (projectilePrefab == null || firePoint == null) return;
        
        // Forzar posición Z para asegurar visibilidad
        Vector3 spawnPos = firePoint.position;
        spawnPos.z = 0;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projScript = proj.GetComponent<Projectile>();
        
        if (projScript != null)
        {
            projScript.Initialize(direction, speed);
        }

        // Asegurar que el SpriteRenderer tenga un orden alto
        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Foreground"; // O la capa que uses delante
            sr.sortingOrder = 100;
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

