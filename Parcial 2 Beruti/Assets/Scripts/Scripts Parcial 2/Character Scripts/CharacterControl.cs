using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    private float currentSpeed;
    private Vector3 moveDirection;

    [Header("Agacharse")]
    public float crouchHeightMultiplier = 2f;
    public float crouchSpeedMultiplier = 0.75f;
    private bool isCrouched = false;
    private float originalHeight;
    private CharacterController controller;

    [Header("Cámara")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;

    [Header("Gravedad")]
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        originalHeight = controller.height;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();
        HandleCamera();
    }

    void HandleMovement()
    {
        // Comprobamos si está tocando el suelo
        isGrounded = controller.isGrounded;

        // Si está en el suelo, mantenemos una pequeña velocidad hacia abajo
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Aplicar gravedad manualmente
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
            ToggleCrouch();
    }

    void ToggleCrouch()
    {
        if (isCrouched)
        {
            controller.height = originalHeight;
            currentSpeed = walkSpeed;
            isCrouched = false;
        }
        else
        {
            controller.height = originalHeight * crouchHeightMultiplier;
            currentSpeed = walkSpeed * crouchSpeedMultiplier;
            isCrouched = true;
        }
    }
}
