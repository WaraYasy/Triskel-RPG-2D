using UnityEngine;

/// <summary>
/// GameConstants - Configuración centralizada de colores y dimensiones
/// ScriptableObject que se puede editar desde el Inspector
/// </summary>
[CreateAssetMenu(fileName = "GameConstants", menuName = "Triskel/Game Constants")]
public class GameConstants : ScriptableObject
{
    [Header("=== COLORES DE ZONAS DE PELIGRO ===")]
    [Tooltip("Color de advertencia (naranja parpadeante)")]
    public Color dangerZoneWarning = new Color(1f, 0.5f, 0f, 0.4f);
    
    [Tooltip("Color de peligro inminente (rojo sólido)")]
    public Color dangerZoneDanger = new Color(1f, 0f, 0f, 0.9f);
    
    [Header("=== COLORES DE RELIQUIAS ===")]
    public Color relicLirio = new Color(0f, 1f, 1f, 1f);      // Cyan
    public Color relicHacha = new Color(1f, 0f, 0f, 1f);      // Rojo
    public Color relicManto = new Color(0.5f, 0f, 0.5f, 1f);  // Púrpura
    
    [Header("=== COLORES DE PROYECTILES ===")]
    public Color projectileNormal = new Color(1f, 0f, 0f, 1f);      // Rojo
    public Color projectileFast = new Color(1f, 0.5f, 0f, 1f);      // Naranja (moral negativa)
    public Color projectileSlow = new Color(0f, 0.5f, 1f, 1f);      // Azul (moral positiva)
    
    [Header("=== DIMENSIONES ===")]
    [Tooltip("Radio de zona de peligro pequeña")]
    public float dangerZoneSmallRadius = 2f;
    
    [Tooltip("Radio de zona de peligro grande")]
    public float dangerZoneLargeRadius = 2.5f;
    
    [Tooltip("Velocidad de movimiento del jugador")]
    public float playerMoveSpeed = 5f;
    
    [Tooltip("Velocidad y distancia del dash")]
    public float playerDashSpeed = 25f;
    
    [Header("=== TIEMPOS ===")]
    [Tooltip("Duración del dash")]
    public float dashDuration = 0.2f;
    
    [Tooltip("Cooldown del dash")]
    public float dashCooldown = 1.5f;
    
    [Tooltip("Tiempo de advertencia de zona de peligro")]
    public float dangerZoneWarningTime = 1.5f;
}
