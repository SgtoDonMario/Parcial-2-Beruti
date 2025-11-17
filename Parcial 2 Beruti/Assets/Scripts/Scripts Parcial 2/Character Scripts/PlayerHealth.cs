using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Referencias UI")]
    public Image healthBarFill;
    public TextMeshProUGUI healthText;

    [Header("Referencias del Jugador")]
    public GameObject playerModel;  // La cápsula o el mesh del jugador
    public GameObject playerGun;    // El arma del jugador

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;

        // Reaparecer modelo
        if (playerModel != null)
            playerModel.SetActive(true);

        // Reactivar movimiento
        GetComponent<PlayerMovement>().enabled = true;

        // Reactivar arma
        if (playerGun != null)
            playerGun.SetActive(true);

        UpdateHealthUI();
    }


    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        Debug.Log($"[PLAYER DAMAGE] - Daño recibido: {amount}  |  Vida restante: {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }


    void Die()
    {
        isDead = true;

        // Desactivar movimiento
        GetComponent<PlayerMovement>().enabled = false;

        // Ocultar cápsula o modelo
        if (playerModel != null)
            playerModel.SetActive(false);

        // Desactivar arma visual y script
        if (playerGun != null)
            playerGun.SetActive(false);

        PlayerWeapon weapon = GetComponent<PlayerWeapon>();
        if (weapon != null)
            weapon.enabled = false;

        Debug.Log("Jugador muerto");
    }

    public void Heal(float amount)
    {
        if (isDead) return;

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
