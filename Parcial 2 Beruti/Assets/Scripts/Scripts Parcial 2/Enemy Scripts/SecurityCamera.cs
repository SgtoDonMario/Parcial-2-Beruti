using UnityEngine;
using TMPro;

public class SecurityCamera : MonoBehaviour
{
    public enum CameraState { Vigilando, Alerta }
    public CameraState currentState = CameraState.Vigilando;

    [Header("Vida")]
    public float maxHealth = 50f;
    private float currentHealth;
    private bool destroyed = false;

    [Header("Rotación Automática")]
    public float rotationSpeed = 30f;
    public float rotationAngle = 45f;
    public float pauseTime = 1f;

    private float currentRotation = 0f;
    private int direction = 1;
    private float pauseTimer = 0f;

    [Header("Detección")]
    public float visionDistance = 12f;
    public float visionAngle = 60f;
    public LayerMask obstacleMask;

    [Header("UI")]
    public TextMeshPro statusText;
    private Vector3 uiOffset = new Vector3(0, 1.5f, 0);

    public Transform player;

    void Start()
    {
        currentHealth = maxHealth;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        UpdateStatusText("Vigilando");
    }

    void Update()
    {
        if (destroyed) return;

        UpdateBillboard();
        ScanMovement();
        DetectPlayer();
    }

    // ROTACIÓN
    void ScanMovement()
    {
        if (currentState == CameraState.Alerta)
            return; // cuando está en alerta deja de rotar

        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float delta = rotationSpeed * Time.deltaTime * direction;
        currentRotation += delta;

        transform.rotation *= Quaternion.Euler(0, delta, 0);

        if (Mathf.Abs(currentRotation) >= rotationAngle)
        {
            direction *= -1;
            currentRotation = Mathf.Clamp(currentRotation, -rotationAngle, rotationAngle);
            pauseTimer = pauseTime;
        }
    }

    // DETECCIÓN
    void DetectPlayer()
    {
        if (player == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        // Dentro del cono de visión
        if (angle < visionAngle / 2f && distance < visionDistance)
        {
            // Sin obstáculos en el medio
            if (!Physics.Raycast(transform.position, dirToPlayer, distance, obstacleMask))
            {
                EnterAlertState();
                return;
            }
        }

        // Si no lo ve → vuelve a vigilando
        if (currentState != CameraState.Alerta)
            return;

        // Si había estado en alerta pero ya no ve al jugador
        currentState = CameraState.Vigilando;
        UpdateStatusText("Vigilando");
    }

    // ENTRAR EN ALERTA
    void EnterAlertState()
    {
        if (currentState == CameraState.Alerta)
            return;

        currentState = CameraState.Alerta;
        UpdateStatusText("ALERTA!");

        AlertEnemies();
    }

    // ALERTAR A TODOS LOS ENEMIGOS
    void AlertEnemies()
    {
        EnemyController[] enemies = FindObjectsOfType<EnemyController>();

        foreach (EnemyController enemy in enemies)
        {
            enemy.ReceiveAlert();
        }
    }

    // DAÑO
    public void TakeDamage(float dmg)
    {
        if (destroyed) return;

        currentHealth -= dmg;

        UpdateStatusText("Damage!");

        if (currentHealth <= 0)
            DestroyCamera();
    }

    void DestroyCamera()
    {
        destroyed = true;

        UpdateStatusText("OFFLINE");

        if (statusText != null)
        {
            statusText.transform.SetParent(null);
            Destroy(statusText.gameObject, 1.5f);
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        Destroy(gameObject, 1f);
    }

    // BILLBOARD
    void UpdateBillboard()
    {
        if (statusText == null || Camera.main == null) return;

        statusText.transform.position = transform.position + uiOffset;

        Vector3 dir = statusText.transform.position - Camera.main.transform.position;
        statusText.transform.rotation = Quaternion.LookRotation(dir);
    }

    void UpdateStatusText(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionDistance);

        Vector3 leftLimit = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightLimit = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftLimit * visionDistance);
        Gizmos.DrawRay(transform.position, rightLimit * visionDistance);
    }
}
