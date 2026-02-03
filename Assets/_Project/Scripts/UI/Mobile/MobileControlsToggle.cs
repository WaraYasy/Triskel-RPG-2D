using UnityEngine;

/// <summary>
/// Muestra u oculta los controles móviles dependiendo de la plataforma.
/// </summary>
public class MobileControlsToggle : MonoBehaviour
{
    [SerializeField] private bool showInEditor = true;
    [Header("Elementos exclusivos de móvil (Joystick, botones)")]
    [SerializeField] private GameObject[] mobileOnlyElements;

    void Awake()
    {
        bool shouldShowMobile = false;

        if (Application.isEditor)
        {
            shouldShowMobile = showInEditor;
        }
        else
        {
#if UNITY_ANDROID || UNITY_IOS
            shouldShowMobile = true;
#else
            shouldShowMobile = false;
#endif
        }

        // En lugar de ocultar TODO el objeto (que incluye la vida),
        // ocultamos solo lo que sea exclusivo de móvil.
        if (!shouldShowMobile)
        {
            foreach (GameObject element in mobileOnlyElements)
            {
                if (element != null)
                {
                    element.SetActive(false);
                }
            }
        }
    }
}
