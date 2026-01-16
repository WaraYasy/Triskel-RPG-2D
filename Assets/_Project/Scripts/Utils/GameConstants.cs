using UnityEngine;

public enum DialogueTheme { Oscuro, Claro }

/// <summary>
/// GameConstants - Configuración centralizada de colores y dimensiones
/// ScriptableObject que se puede editar desde el Inspector
/// </summary>
[CreateAssetMenu(fileName = "GameConstants", menuName = "Triskel/Game Constants")]
public class GameConstants : ScriptableObject
{
    [Header("=== TEMA OSCURO ===")]
    public Color oscuro_Fondo = new Color(0f, 0f, 0f, 1f);
    public Color oscuro_Borde = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color oscuro_Texto = Color.white;
    public Color oscuro_NombrePersonaje = new Color(0.85f, 0.65f, 0.25f, 1f);
    public Color oscuro_Boton = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color oscuro_BotonHover = new Color(0.25f, 0.22f, 0.18f, 1f);

    [Header("=== TEMA CLARO ===")]
    public Color claro_Fondo =  new Color(0f, 0f, 0f, 1f);
    public Color claro_Borde = new Color(0.6f, 0.55f, 0.45f, 1f);
    public Color claro_Texto = new Color(0.12f, 0.12f, 0.12f, 1f);
    public Color claro_NombrePersonaje = new Color(0.55f, 0.35f, 0.15f, 1f);
    public Color claro_Boton = new Color(0.85f, 0.82f, 0.75f, 1f);
    public Color claro_BotonHover = new Color(0.75f, 0.70f, 0.60f, 1f);

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
