using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerHealthUI - Actualiza una barra de vida (Slider o Image) basada en PlayerHealth.
/// </summary>
public class PlayerHealthUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateUI);
            UpdateUI(playerHealth.GetCurrentHealth() / playerHealth.GetMaxHealth());
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateUI);
        }
    }

    public void UpdateUI(float healthPercent)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercent;
            fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercent);
        }
    }
}
