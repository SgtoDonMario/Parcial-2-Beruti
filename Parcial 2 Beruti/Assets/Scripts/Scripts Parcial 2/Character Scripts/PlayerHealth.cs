using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Referencias UI")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update()
    {
        // 🔹 Simular daño al presionar H
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f); // Resta 10 puntos de vida
        }

        // 🔹 (Opcional) Curar al presionar J
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(10f);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = Mathf.RoundToInt((currentHealth / maxHealth) * 100) + "%";
    }
}
