using UnityEngine;

/// <summary>
/// Muestra u oculta los controles móviles dependiendo de la plataforma.
/// </summary>
public class MobileControlsToggle : MonoBehaviour
{
    [SerializeField] private bool showInEditor = true;

    void Awake()
    {
        // En el editor, respetamos la variable para testear
        if (Application.isEditor)
        {
            gameObject.SetActive(showInEditor);
            return;
        }

        // Para exportaciones (Builds)
#if UNITY_ANDROID
        // Si es Android, activamos
        gameObject.SetActive(true);
#else
        // En cualquier otra plataforma (PC, WebGL, iOS, etc.), desactivamos/ocultamos
        gameObject.SetActive(false);
#endif
    }
}
