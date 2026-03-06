using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image staminaBarImage;

    [Header("Stamina Warning")]
    [SerializeField] private Color normalStaminaColor = Color.cyan;
    [SerializeField] private Color exhaustedStaminaColor = Color.red;
    [SerializeField] private bool changeStaminaColorOnExhausted = true;

    [Header("Health Warning")]
    [SerializeField] private Color normalHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 30f;

    [Header("Player References")]
    [SerializeField] private PlayerController playerController;

    void Start()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerController has not been found!");
            }
        }

        if (staminaBarImage != null && changeStaminaColorOnExhausted)
        {
            staminaBarImage.color = normalStaminaColor;
        }

        if (healthBarImage != null)
        {
            healthBarImage.color = normalHealthColor;
        }
    }

    void Update()
    {
        if (playerController == null) return;
        if (healthBarImage != null)
        {
            float healthPercent = playerController.CurrentHealth / playerController.MaxHealth;
            healthBarImage.fillAmount = healthPercent;
            if (healthPercent <= lowHealthThreshold / 100f)
            {
                healthBarImage.color = lowHealthColor;
            }
            else
            {
                healthBarImage.color = normalHealthColor;
            }
        }

        if (staminaBarImage != null)
        {
            staminaBarImage.fillAmount = playerController.CurrentStamina / playerController.MaxStamina;
            if (changeStaminaColorOnExhausted)
            {
                staminaBarImage.color = playerController.IsExhausted ? exhaustedStaminaColor : normalStaminaColor;
            }
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = currentHealth / maxHealth;
        }
    }

    public void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaBarImage != null)
        {
            staminaBarImage.fillAmount = currentStamina / maxStamina;
        }
    }
}