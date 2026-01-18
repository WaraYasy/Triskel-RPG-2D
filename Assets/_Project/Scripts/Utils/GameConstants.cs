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
    public Color oscuro_NombrePersonaje = new Color(0.91f, 0.84f, 0.42f, 1f);  // Dorado de la estrella
    public Color oscuro_Boton = new Color(0.24f, 0.42f, 0.48f, 1f);            // Teal oscuro (#3E6B7A) - contraste 5.2:1 con texto blanco
    public Color oscuro_BotonHover = new Color(0.71f, 0.57f, 0.24f, 1f);       // Dorado hover (#B5913D) - contraste 4.6:1 con texto blanco

    [Header("=== TEMA CLARO ===")]
    public Color claro_Fondo =  new Color(0f, 0f, 0f, 1f);
    public Color claro_Borde = new Color(0.50f, 0.70f, 0.75f, 1f);             // Teal claro como borde
    public Color claro_Texto = new Color(0.12f, 0.12f, 0.12f, 1f);
    public Color claro_NombrePersonaje = new Color(0.24f, 0.42f, 0.48f, 1f);   // Teal oscuro
    public Color claro_Boton = new Color(0.50f, 0.70f, 0.75f, 1f);             // Teal claro (#7FB3C0) - contraste 6.8:1 con texto oscuro
    public Color claro_BotonHover = new Color(0.91f, 0.84f, 0.42f, 1f);        // Dorado hover - contraste 12:1 con texto oscuro

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

    [Header("=== COLORES DEL DIARIO (UI) ===")]
    [Tooltip("Fondo del botón de navegación - estado normal")]
    public Color diaryNavButton = new Color(0.23f, 0.10f, 0.05f, 1f);           // rgb(59, 26, 13) - marrón oscuro

    [Tooltip("Borde del botón - estado normal")]
    public Color diaryNavButtonBorder = new Color(0.59f, 0.55f, 0.47f, 1f);     // rgb(150, 140, 120)

    [Tooltip("Fondo del botón - hover (dorado brillante)")]
    public Color diaryNavButtonHover = new Color(0.71f, 0.47f, 0.16f, 1f);      // rgb(180, 120, 40)

    [Tooltip("Borde del botón - hover")]
    public Color diaryNavButtonBorderHover = new Color(0.90f, 0.78f, 0.47f, 1f); // rgb(230, 200, 120)

    [Tooltip("Fondo del botón - active (muy oscuro, presionado)")]
    public Color diaryNavButtonActive = new Color(0.12f, 0.06f, 0.02f, 1f);     // rgb(30, 15, 5)

    [Tooltip("Borde del botón - active")]
    public Color diaryNavButtonBorderActive = new Color(0.39f, 0.31f, 0.20f, 1f); // rgb(100, 80, 50)

    [Tooltip("Fondo del botón - disabled (gris neutro)")]
    public Color diaryNavButtonDisabled = new Color(0.31f, 0.29f, 0.27f, 1f);   // rgb(80, 75, 70)

    [Tooltip("Borde del botón - disabled")]
    public Color diaryNavButtonBorderDisabled = new Color(0.39f, 0.37f, 0.35f, 1f); // rgb(100, 95, 90)

    [Tooltip("Texto del botón - disabled")]
    public Color diaryNavButtonTextDisabled = new Color(0.51f, 0.49f, 0.47f, 1f); // rgb(130, 125, 120)
    
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
