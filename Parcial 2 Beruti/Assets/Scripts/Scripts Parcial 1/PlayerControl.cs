using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float maxStamina = 10f;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 300f;

    [Header("Weapon")]
    public float fireRate = 0.5f;
    public float weaponRange = 10f;
    public float weaponDamage = 20f;

    private CharacterController controller;
    private Vector3 velocity;
    private float currentHealth;
    private float currentStamina;
    private float nextFireTime;
    private float staminaLogTimer = 0f;
    private float staminaTickTimer = 0f;
    private float xRotation = 0f;

    private EnemyAI enemy;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        currentStamina = maxStamina;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        enemy = FindObjectOfType<EnemyAI>();
    }

    private void Update()
    {
        if (enemy == null)
            enemy = FindObjectOfType<EnemyAI>();

        HandleMouseLook();
        HandleMovement();
        HandleWeapon();
        HandleStamina();

        // imprimir stamina una vez por segundo
        staminaLogTimer += Time.deltaTime;
        if (staminaLogTimer >= 1f)
        {
            Debug.Log("Stamina: " + currentStamina.ToString("F2"));
            staminaLogTimer = 0f;
        }

        if (Input.GetKeyDown(KeyCode.F3) && enemy != null && !enemy.gameObject.activeSelf)
            enemy.Respawn();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        float speed = isRunning ? runSpeed : walkSpeed;

        // Movimiento horizontal
        controller.Move(move * speed * Time.deltaTime);

        // ✅ Aplicar gravedad de forma estable
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // mantiene al jugador "pegado" al suelo
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Aplicar gravedad constante
        velocity.y += gravity * Time.deltaTime;

        // Movimiento vertical
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleWeapon()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime) // 🔧 cambio a semiautomático
        {
            nextFireTime = Time.time + fireRate;

            Collider[] hits = Physics.OverlapSphere(transform.position, weaponRange);
            foreach (var hit in hits)
            {
                EnemyAI enemyHit = hit.GetComponent<EnemyAI>();
                if (enemyHit != null)
                    enemyHit.TakeDamage(weaponDamage);
            }
        }
    }

    void HandleStamina()
    {
        staminaTickTimer += Time.deltaTime;

        if (staminaTickTimer < 1f)
            return;

        staminaTickTimer = 0f;

        if (enemy == null || enemy.CurrentState == EnemyAI.EnemyState.Dead)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + 1f);
            return;
        }

        if (enemy.CurrentState == EnemyAI.EnemyState.Chasing)
        {
            currentStamina = Mathf.Max(0f, currentStamina - 1f);
        }
        else
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + 1f);
        }
    }
}

