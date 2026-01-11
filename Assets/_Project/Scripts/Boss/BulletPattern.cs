using UnityEngine;

/// <summary>
/// BulletPattern - Patrones de disparo del boss
/// Versión 1.0 - Patrones básicos (circular, línea, espiral)
/// </summary>
public class BulletPattern : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    
    [Header("Configuración de Patrones")]
    [SerializeField] private float baseProjectileSpeed = 5f;
    [SerializeField] private float fireRate = 1f;
    
    private float nextFireTime = 0f;
    
    public enum PatternType
    {
        Circle,      // Disparo circular (360°)
        Line,        // Disparo en línea recta
        Spread,      // Abanico de proyectiles
        Spiral       // Espiral rotante
    }
    
    private void Update()
    {
        if (Time.time >= nextFireTime)
        {
            // Alternar entre patrones
            int randomPattern = Random.Range(0, 3);
            FirePattern((PatternType)randomPattern);
            
            nextFireTime = Time.time + fireRate;
        }
    }
    
    public void FirePattern(PatternType pattern)
    {
        switch (pattern)
        {
            case PatternType.Circle:
                FireCircle(12); // 12 proyectiles en círculo
                break;
            
            case PatternType.Line:
                FireLine(Vector2.down);
                break;
            
            case PatternType.Spread:
                FireSpread(5, 60f); // 5 proyectiles en 60°
                break;
            
            case PatternType.Spiral:
                FireSpiral(8);
                break;
        }
    }
    
    // Patrón circular (360°)
    private void FireCircle(int count)
    {
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            SpawnProjectile(direction, baseProjectileSpeed);
        }
    }
    
    // Patrón en línea
    private void FireLine(Vector2 direction)
    {
        SpawnProjectile(direction.normalized, baseProjectileSpeed);
    }
    
    // Patrón de abanico
    private void FireSpread(int count, float spreadAngle)
    {
        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (count - 1);
        
        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + (i * angleStep);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.down;
            SpawnProjectile(direction, baseProjectileSpeed);
        }
    }
    
    // Patrón espiral
    private void FireSpiral(int count)
    {
        float angleOffset = Time.time * 50f; // Rotación constante
        float angleStep = 360f / count;
        
        for (int i = 0; i < count; i++)
        {
            float angle = (i * angleStep) + angleOffset;
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            SpawnProjectile(direction, baseProjectileSpeed);
        }
    }
    
    // Spawn de proyectil
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
    
    // Método público para cambiar velocidad (moral)
    public void SetProjectileSpeed(float speedMultiplier)
    {
        baseProjectileSpeed = 5f * speedMultiplier;
    }
}
