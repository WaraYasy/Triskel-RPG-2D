using UnityEngine;

/// <summary>
/// Muestra u oculta los controles móviles dependiendo de la plataforma.
/// </summary>
public class MobileControlsToggle : MonoBehaviour
{
    [SerializeField] private bool showInEditor = true;

    void Awake()
    {
        // En el editor, podemos elegir si queremos verlos para testear
        if (Application.isEditor)
        {
            gameObject.SetActive(showInEditor);
            return;
        }

        // En otros casos, solo se activan si es plataforma móvil
        // Esto incluye Android, iOS, Windows Phone, etc.
        bool isMobile = Application.isMobilePlatform;
        
        gameObject.SetActive(isMobile);
    }
}
