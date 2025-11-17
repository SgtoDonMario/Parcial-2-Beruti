using UnityEngine;
using UnityEngine.SceneManagement;

public class EventManager : MonoBehaviour
{
    public static bool isPaused = false;
    public int testNumber = 5;
    [Header("Respawn del Jugador")]
    public Transform playerRespawnPoint;
    public PlayerHealth playerHealth;
    public PlayerWeapon playerGun;  // tu script de armas (cámbialo por el correcto)

    void Update()
    {
        HandlePause();
        HandleDeveloperKeys();
    }

    // ------------------------------------
    //              PAUSA
    // ------------------------------------
    void HandlePause()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ------------------------------------
    // F1 → Respawn jugador
    // F2 → Reset escena
    // ------------------------------------
    void HandleDeveloperKeys()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RespawnPlayer();
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            ResetScene();
        }
    }

    // ------------------------------------
    //         RESPAWN DEL JUGADOR
    // ------------------------------------
    void RespawnPlayer()
    {
        if (playerHealth != null)
            playerHealth.ResetHealth();  // debes agregar este método

        if (playerGun != null)
            playerGun.ResetAmmo();       // debes agregar este método

        if (playerRespawnPoint != null)
            playerHealth.transform.position = playerRespawnPoint.position;

        playerHealth.GetComponent<PlayerMovement>().enabled = true;

        PlayerWeapon gun = playerHealth.GetComponent<PlayerWeapon>();
        if (gun != null)
            gun.enabled = true;
    }

    // ------------------------------------
    //         RESET DE LA ESCENA
    // ------------------------------------
    void ResetScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
