using UnityEngine;
using TMPro;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Patrol, Alert, Chase, Damage, Dead }
    public EnemyState currentState = EnemyState.Patrol;
    public Transform weapon;      // referencia al arma del enemigo
    public CharacterController playerController;

    [Header("Referencias")]
    public EnemyData enemyData;
    public Transform player;
    public TextMeshPro statusText;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    private Transform patrolTarget;
    private int patrolIndex = 0;

    [Header("Ataque a distancia")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootRange = 10f;
    public float fireRate = 1f;
    public float projectileDamage = 10f;

    private float nextFireTime = 0f;

    [Header("IA")]
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Physics / Ajustes")]
    public float rbMass = 300f;
    public float avoidanceDistance = 0.6f;
    public float lateralCheckDistance = 1.0f;
    public float stopDistance = 2f;
    public float maxChaseSpeed = 3f;

    private float currentHealth;
    private bool dying = false;
    private float lastDamageTime;

    private Rigidbody enemyRb;
    private Vector3 fixedMoveDelta = Vector3.zero;
    private Vector3 uiOffset = new Vector3(0, 2f, 0);

    private float destinationUpdateTimer = 0f;

    void Start()
    {
        enemyRb = GetComponent<Rigidbody>();
        if (enemyRb == null) enemyRb = gameObject.AddComponent<Rigidbody>();

        enemyRb.useGravity = false;
        enemyRb.isKinematic = false;
        enemyRb.constraints = RigidbodyConstraints.FreezeRotation;
        enemyRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        enemyRb.interpolation = RigidbodyInterpolation.Interpolate;
        enemyRb.mass = rbMass;
        currentHealth = enemyData != null ? enemyData.maxHealth : 100f;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (statusText != null)
            statusText.text = currentState.ToString();
    }

    void Update()
    {
        UpdateBillboard();
        if (dying) return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolLogic();    // NUEVO
                DetectPlayer();
                fixedMoveDelta = Vector3.zero;
                break;
            case EnemyState.Alert:
            case EnemyState.Damage:
                DetectPlayer();
                fixedMoveDelta = Vector3.zero;
                break;

            case EnemyState.Chase:
                ChaseLogic();
                break;
        }

        AimWeaponAtPlayer();

    }

    void AimWeaponAtPlayer()
    {
        if (weapon != null && player != null && playerController != null)
        {
            float eyeHeight = playerController.height * 0.35f;
            Vector3 targetPoint = player.position + Vector3.up * eyeHeight;
            weapon.LookAt(targetPoint);
        }
    }

    // ---------------------------------
    //          PATRULLA
    // ---------------------------------
    void PatrolLogic()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[patrolIndex];

        // Dirección hacia el punto
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        // Rotación suave
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 3f * Time.deltaTime);
        }

        // Movimiento
        transform.position += direction.normalized * patrolSpeed * Time.deltaTime;

        // ¿Llegó al punto?
        if (Vector3.Distance(transform.position, target.position) < 1.0f)
        {
            patrolIndex++;

            // Si llega al final, vuelve al inicio → Ruta circular
            if (patrolIndex >= patrolPoints.Length)
            {
                patrolIndex = 0;
            }
        }
    }
    void HandleShooting()
    {
        if (player == null) return;

        // Distancia al jugador
        float distance = Vector3.Distance(transform.position, player.position);

        // Si está muy lejos, no dispara
        if (distance > shootRange) return;

        // Cadencia de fuego
        if (Time.time < nextFireTime) return;

        Shoot();
        nextFireTime = Time.time + fireRate;
    }
    void Shoot()
    {
        // Instanciar bala
        GameObject b = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Setear dueño y daño
        Projectile p = b.GetComponent<Projectile>();
        p.owner = gameObject;
        p.damage = projectileDamage;
    }

    void FixedUpdate()
    {
        if (dying) return;

        if (currentState == EnemyState.Chase)
        {
            // Si el desplazamiento es muy pequeño, no llamar MovePosition
            if (fixedMoveDelta.sqrMagnitude > 0f && !float.IsNaN(fixedMoveDelta.sqrMagnitude))
            {
                // Mover con MovePosition pero limitando la distancia máxima por paso
                // Esto previene movimientos explosivos si algo sale mal
                float maxStep = 10f * Time.fixedDeltaTime; // por seguridad, 10 m/s tope (ajustable)
                float stepMag = fixedMoveDelta.magnitude;
                Vector3 step = fixedMoveDelta;
                if (stepMag > maxStep)
                    step = fixedMoveDelta.normalized * maxStep;

                enemyRb.MovePosition(enemyRb.position + step);
            }
        }
    }

    // -------------------------
    //       BILLBOARD
    // -------------------------
    void UpdateBillboard()
    {
        if (statusText == null || Camera.main == null) return;

        statusText.transform.position = transform.position + uiOffset;
        Vector3 dir = statusText.transform.position - Camera.main.transform.position;
        statusText.transform.rotation = Quaternion.LookRotation(dir);
        statusText.text = currentState.ToString();
    }

    // -------------------------
    //       DETECCIÓN
    // -------------------------
    void DetectPlayer()
    {
        if (player == null || enemyData == null) return;

        Vector3 eyes = transform.position + Vector3.up * 1.25f;
        Vector3 dirToPlayer = (player.position - eyes).normalized;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        float distance = Vector3.Distance(eyes, player.position);

        if (angle < enemyData.visionAngle / 2f && distance < enemyData.visionDistance)
        {
            RaycastHit hit;

            // Usamos BOTH: obstaculos + jugador
            if (Physics.Raycast(eyes, dirToPlayer, out hit, distance, obstacleMask | playerMask))
            {
                // Si LO PRIMERO que toca el raycast es el jugador, entonces lo ve
                if (hit.collider.transform == player)
                {
                    EnterAlert();
                }
            }
        }
    }

    // -------------------------
    //     CHASE + MOVIMIENTO
    // -------------------------
    void ChaseLogic()
    {
        if (player == null || enemyData == null)
        {
            fixedMoveDelta = Vector3.zero;
            return;
        }

        // Dirección horizontal al jugador
        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;
        if (distance <= 0.001f)
        {
            fixedMoveDelta = Vector3.zero;
            return;
        }

        // Rotación suave hacia el jugador
        Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 5f * Time.deltaTime);

        // Mantener distancia mínima
        if (distance <= stopDistance)
        {
            fixedMoveDelta = Vector3.zero;
            return;
        }

        // ---- velocidad segura ----
        // Tomamos la velocidad de enemyData, pero la limitamos al max configurado
        float rawSpeed = enemyData.moveSpeed;
        float speed = rawSpeed;
        // Si declaraste public float maxChaseSpeed, úsala; si no existe, 3f por defecto.
        float maxSpeed = (GetType().GetField("maxChaseSpeed") != null) ? (float)GetType().GetField("maxChaseSpeed").GetValue(this) : 3f;
        speed = Mathf.Min(speed, maxSpeed);

        // Calculamos la posición objetivo usando MoveTowards (evita overshoot)
        Vector3 desiredVelocity = direction.normalized * speed; // m/s
        Vector3 desiredPositionThisFixed = enemyRb.position + desiredVelocity * Time.fixedDeltaTime;

        // fixedMoveDelta es el desplazamiento que aplicaremos en FixedUpdate
        fixedMoveDelta = desiredPositionThisFixed - enemyRb.position;

        // Debug opcional: línea y log de velocidad aplicada
        if (GetType().GetField("debugChase") != null && (bool)GetType().GetField("debugChase").GetValue(this))
        {
            Debug.DrawLine(enemyRb.position, enemyRb.position + fixedMoveDelta * 10f, Color.cyan, 0.1f);
            Debug.Log($"[Enemy] moveSpeed raw:{rawSpeed:F2} applied:{speed:F2} fixedDelta:{fixedMoveDelta.magnitude:F3}m");
        }

        HandleShooting();
    }
    

    // -------------------------
    //       ALERTA GLOBAL
    // -------------------------
    void EnterAlert()
    {
        if (currentState == EnemyState.Alert || currentState == EnemyState.Chase) return;

        currentState = EnemyState.Alert;

        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
            enemy.ReceiveAlert();

        Invoke(nameof(StartChase), 0.3f);
    }

    public void ReceiveAlert()
    {
        if (currentState == EnemyState.Dead) return;

        currentState = EnemyState.Chase;

        fixedMoveDelta = Vector3.forward * 0.001f; // evitar quedada estática
    }

    void StartChase()
    {
        currentState = EnemyState.Chase;
    }

    // -------------------------
    //         DAÑO
    // -------------------------
    public void TakeDamage(float dmg)
    {
        if (dying) return;

        currentHealth -= dmg;
        currentState = EnemyState.Damage;
        lastDamageTime = Time.time;

        if (currentHealth <= 0)
            Die();
        else
            Invoke(nameof(DelayedAlert), 3f);
    }

    void DelayedAlert()
    {
        if (currentState == EnemyState.Dead) return;

        foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
            enemy.ReceiveAlert();
    }

    // -------------------------
    //         MUERTE
    // -------------------------
    void Die()
    {
        if (dying) return;
        dying = true;
        currentState = EnemyState.Dead;

        if (statusText != null)
        {
            statusText.text = "Dead";
            statusText.transform.SetParent(null);
            Destroy(statusText.gameObject, 2f);
        }

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        Destroy(gameObject, 1f);
    }

    // -------------------------
    //        GIZMOS
    // -------------------------
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
