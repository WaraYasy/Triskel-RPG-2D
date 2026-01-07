using UnityEngine;

/// <summary>
/// Sistema de Reliquias - Cambio y uso de habilidades
/// Versión 1.0 - Cambio básico entre reliquias con teclas 1/2/3
/// </summary>
public class RelicSystem : MonoBehaviour
{
    public enum RelicType
    {
        None = 0,
        LirioAzul = 1,
        HachaSagrada = 2,
        MantoDeLuna = 3
    }

    [Header("Configuración")]
    [SerializeField] private RelicType currentRelic = RelicType.None;
    
    [Header("Visual Feedback (Opcional)")]
    [SerializeField] private SpriteRenderer relicIndicator; // Sprite para mostrar reliquia activa
    [SerializeField] private Color colorLirio = Color.cyan;
    [SerializeField] private Color colorHacha = Color.red;
    [SerializeField] private Color colorManto = new Color(0.5f, 0f, 0.5f); // Púrpura
    
    private void Update()
    {
        // Cambio de reliquia con teclas numéricas
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectRelic(RelicType.LirioAzul);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectRelic(RelicType.HachaSagrada);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectRelic(RelicType.MantoDeLuna);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            SelectRelic(RelicType.None);
        }
        
        // TODO: Input para usar reliquia (tecla Z)
        // if (Input.GetKeyDown(KeyCode.Z))
        // {
        //     UseCurrentRelic();
        // }
    }
    
    public void SelectRelic(RelicType relic)
    {
        currentRelic = relic;
        
        // Visual feedback
        UpdateVisualFeedback();
        
        // Log para debug
        Debug.Log($"Reliquia seleccionada: {relic}");
    }
    
    private void UpdateVisualFeedback()
    {
        if (relicIndicator == null) return;
        
        switch (currentRelic)
        {
            case RelicType.None:
                relicIndicator.color = Color.white;
                relicIndicator.enabled = false;
                break;
            
            case RelicType.LirioAzul:
                relicIndicator.color = colorLirio;
                relicIndicator.enabled = true;
                break;
            
            case RelicType.HachaSagrada:
                relicIndicator.color = colorHacha;
                relicIndicator.enabled = true;
                break;
            
            case RelicType.MantoDeLuna:
                relicIndicator.color = colorManto;
                relicIndicator.enabled = true;
                break;
        }
    }
    
    // Método para usar la reliquia actual
    private void UseCurrentRelic()
    {
        if (currentRelic == RelicType.None)
        {
            Debug.LogWarning("No hay reliquia equipada");
            return;
        }
        
        // TODO: Implementar habilidades por reliquia
        Debug.Log($"Usando reliquia: {currentRelic}");
    }
    
    // Métodos públicos para acceder al estado
    public RelicType GetCurrentRelic() => currentRelic;
    public bool HasRelicEquipped() => currentRelic != RelicType.None;
}
