using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    private float currentSpeed;

    [Header("Agacharse (Nuevo sistema por escala)")]
    public float crouchScale = 0.5f;      // Escala al 50%
    public float crouchSpeedMultiplier = 0.75f;
    private bool isCrouched = false;
    private Vector3 originalScale;

    private CharacterController controller;

    [Header("Cámara")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;

    [Header("Gravedad")]
    public float gravity = -9.81f;
    private Vector3 velocity;

    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;

        originalScale = transform.localScale;

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
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
    }

    // -------------------------------------------
    // NUEVO SISTEMA DE AGACHADO POR ESCALA
    // -------------------------------------------
    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleCrouch();
        }
    }

    void ToggleCrouch()
    {
        if (!isCrouched)
        {
            // AGACHAR
            transform.localScale = new Vector3(
                originalScale.x,
                originalScale.y * crouchScale,
                originalScale.z
            );

            currentSpeed = walkSpeed * crouchSpeedMultiplier;
            isCrouched = true;
        }
        else
        {
            // LEVANTAR
            transform.localScale = originalScale;
            currentSpeed = walkSpeed;
            isCrouched = false;
        }
    }

}

