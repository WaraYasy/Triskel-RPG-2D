using UnityEngine;
using System.Collections;

/// <summary>
/// Muestra u oculta los controles móviles dependiendo de la plataforma.
/// Se usa una corrutina en Start para evitar crashes nativos en Unity 6 al inicializar la UI.
/// </summary>
public class MobileControlsToggle : MonoBehaviour
{
    [SerializeField] private bool showInEditor = true;
    [Header("Elementos exclusivos de móvil (Joystick, botones)")]
    [SerializeField] private GameObject[] mobileOnlyElements;

    IEnumerator Start()
    {
        // CRÍTICO: Esperar un frame permite que Unity termine de inicializar la UI y el Input System.
        // Esto evita el crash "SetActive_Injected" que ocurre en Unity 6 al hacerlo en Awake/Start directo.
        yield return null;

        bool isMobile = false;

        // 1. Determinar si debemos mostrar controles
        if (Application.isEditor)
        {
            isMobile = showInEditor;
        }
        else
        {
#if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
#else
            isMobile = false; 
#endif
        }

        // 2. Aplicar lógica de forma segura
        if (mobileOnlyElements != null)
        {
            foreach (GameObject element in mobileOnlyElements)
            {
                if (element == null) continue;

                try
                {
                    // Solo llamamos a SetActive si el estado es diferente para minimizar llamadas nativas
                    if (element.activeSelf != isMobile)
                    {
                        element.SetActive(isMobile);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[MobileControlsToggle] Error al cambiar estado: {e.Message}");
                }
            }
        }
    }
}