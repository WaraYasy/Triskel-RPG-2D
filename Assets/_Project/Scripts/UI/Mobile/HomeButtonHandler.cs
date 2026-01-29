using UnityEngine;
using UnityEngine.UI;
using Triskel.UI;

/// <summary>
/// Maneja el botón Home de los controles móviles.
/// Busca el PauseController dinámicamente para funcionar con UI persistente.
/// </summary>
public class HomeButtonHandler : MonoBehaviour
{
    private Button homeButton;

    private void Awake()
    {
        homeButton = GetComponent<Button>();

        if (homeButton != null)
        {
            // Limpiar listeners anteriores y agregar el nuevo
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(OnHomePressed);
        }
    }

    private void OnHomePressed()
    {
        // Buscar PauseController a través del UIManager (preferido)
        if (UIManager.Instance != null && UIManager.Instance.PauseController != null)
        {
            UIManager.Instance.PauseController.Pause();
            return;
        }

        // Fallback: buscar directamente
        var pauseController = FindFirstObjectByType<PauseController>();
        if (pauseController != null)
        {
            pauseController.Pause();
        }
        else
        {
            Debug.LogWarning("[HomeButtonHandler] No se encontró PauseController");
        }
    }
}
