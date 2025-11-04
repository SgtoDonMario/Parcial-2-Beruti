using UnityEngine;
using TMPro;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Patrol, Alert, Chase, Damage, Dead }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Referencias")]
    public EnemyData enemyData;
    public Transform player;
    public TextMeshPro statusText;

    [Header("IA")]
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    private float currentHealth;
    private bool dying = false;
    private bool alerted = false;
    private float lastDamageTime;

    private Rigidbody enemyRb;
    private Vector3 fixedMoveDelta = Vector3.zero;
    private Vector3 uiOffset = new Vector3(0, 2f, 0); // 🔹 altura del cartel

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        if (enemyRb == null)
            enemyRb = gameObject.AddComponent<Rigidbody>();

        // 🔹 No queremos física real, solo control manual
        enemyRb.useGravity = false;
        enemyRb.isKinematic = true;
        enemyRb.constraints = RigidbodyConstraints.FreezeRotation;

        currentHealth = enemyData != null ? enemyData.maxHealth : 100f;
    }

    void Update()
    {
        UpdateBillboard();

        if (dying) return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                DetectPlayer();
                fixedMoveDelta = Vector3.zero;
                break;

            case EnemyState.Alert:
                DetectPlayer();
                fixedMoveDelta = Vector3.zero;
                break;

            case EnemyState.Chase:
                CalculateChaseMovement();
                break;

            case EnemyState.Damage:
                DetectPlayer();
                fixedMoveDelta = Vector3.zero;
                break;
        }
    }

    void FixedUpdate()
    {
        if (dying) return;

        // 🔹 Movimiento manual (no físico)
        if (currentState == EnemyState.Chase && fixedMoveDelta != Vector3.zero)
        {
            transform.position += fixedMoveDelta;
        }
    }

    void UpdateBillboard()
    {
        if (statusText != null && Camera.main != null)
        {
            // 🔹 Mantenerlo sobre la cabeza del enemigo
            statusText.transform.position = transform.position + uiOffset;

            // 🔹 Rotarlo hacia la cámara
            Vector3 dir = statusText.transform.position - Camera.main.transform.position;
            statusText.transform.rotation = Quaternion.LookRotation(dir);

            // 🔹 Actualizar el texto
            statusText.text = currentState.ToString();
        }
    }

    void DetectPlayer()
    {
        if (player == null || enemyData == null) return;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        float distance = Vector3.Distance(transform.position, player.position);

        if (angle < enemyData.visionAngle / 2f && distance < enemyData.visionDistance)
        {
            if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, dirToPlayer, distance, obstacleMask))
            {
                EnterAlert();
            }
        }
    }

    void CalculateChaseMovement()
    {
        if (player == null || enemyData == null)
        {
            fixedMoveDelta = Vector3.zero;
            return;
        }

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        float distance = dir.magnitude;

        if (distance > 2f)
        {
            Vector3 dirNorm = dir.normalized;
            Vector3 move = dirNorm * enemyData.moveSpeed * Time.deltaTime;
            fixedMoveDelta = move;

            // Rotación suave hacia el jugador
            Quaternion lookRotation = Quaternion.LookRotation(dirNorm);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            fixedMoveDelta = Vector3.zero;
            Vector3 lookDir = dir;
            lookDir.y = 0f;
            if (lookDir != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(lookDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
        }
    }

    void EnterAlert()
    {
        if (currentState == EnemyState.Alert || currentState == EnemyState.Chase) return;

        currentState = EnemyState.Alert;
        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
        {
            enemy.ReceiveAlert();
        }

        Invoke(nameof(StartChase), 0.5f);
    }

    public void ReceiveAlert()
    {
        if (currentState != EnemyState.Chase && currentState != EnemyState.Dead)
            currentState = EnemyState.Chase;
    }

    void StartChase()
    {
        currentState = EnemyState.Chase;
    }

    public void TakeDamage(float dmg)
    {
        if (dying) return;

        currentHealth -= dmg;
        currentState = EnemyState.Damage;
        lastDamageTime = Time.time;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Invoke(nameof(CheckForDeathSilently), 3f);
        }
    }

    void CheckForDeathSilently()
    {
        if (currentState != EnemyState.Dead && Time.time - lastDamageTime >= 3f)
        {
            foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
                enemy.ReceiveAlert();
        }
    }

    void Die()
    {
        if (dying) return;
        dying = true;
        currentState = EnemyState.Dead;

        if (statusText != null)
        {
            statusText.text = "Dead";

            // 🔹 Desvincular el cartel al morir, para que se quede flotando
            statusText.transform.SetParent(null);
            Destroy(statusText.gameObject, 2.0f);
        }

        // 🔹 Apagar la visibilidad del enemigo antes de destruirlo
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.visionDistance);

        Vector3 leftLimit = Quaternion.Euler(0, -enemyData.visionAngle / 2f, 0) * transform.forward;
        Vector3 rightLimit = Quaternion.Euler(0, enemyData.visionAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, leftLimit * enemyData.visionDistance);
        Gizmos.DrawRay(transform.position, rightLimit * enemyData.visionDistance);
    }
}
