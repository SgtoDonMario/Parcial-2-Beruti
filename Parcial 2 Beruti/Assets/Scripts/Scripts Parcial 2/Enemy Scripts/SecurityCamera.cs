using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Detección")]
    public float visionDistance = 10f;
    public float visionAngle = 45f;
    public LayerMask obstacleMask;

    [Header("Rotación")]
    public float rotationSpeed = 40f;
    public float leftLimit = -45f;
    public float rightLimit = 45f;
    public float pauseTime = 1f;

    [Header("Vida")]
    public float health = 50f;

    private Transform player;
    private float currentRotation = 0f;
    private bool rotatingRight = true;
    private float pauseTimer = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null)
            return;

        RotateCamera();
        DetectPlayer();
    }

    // ----------------------------------------------------------------
    // ROTACIÓN IZQUIERDA / DERECHA
    // ----------------------------------------------------------------
    void RotateCamera()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float step = rotationSpeed * Time.deltaTime;

        if (rotatingRight)
            currentRotation += step;
        else
            currentRotation -= step;

        transform.localRotation = Quaternion.Euler(0, currentRotation, 0);

        if (currentRotation >= rightLimit)
        {
            rotatingRight = false;
            pauseTimer = pauseTime;
        }
        else if (currentRotation <= leftLimit)
        {
            rotatingRight = true;
            pauseTimer = pauseTime;
        }
    }

    // ----------------------------------------------------------------
    // DETECCIÓN DEL PLAYER
    // ----------------------------------------------------------------
    void DetectPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        float distance = Vector3.Distance(transform.position, player.position);

        if (angle < visionAngle / 2f && distance <= visionDistance)
        {
            // raycast para evitar ver a través de paredes
            if (!Physics.Raycast(transform.position, dirToPlayer, distance, obstacleMask))
            {
                Debug.Log("PLAYER DETECTADO por cámara");
                // aquí llamás a los enemigos para perseguir
            }
        }
    }

    // ----------------------------------------------------------------
    // RECIBIR DAÑO Y DESTRUIRSE
    // ----------------------------------------------------------------
    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
