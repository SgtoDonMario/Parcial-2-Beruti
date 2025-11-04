using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Dead }
    public EnemyState CurrentState { get; private set; } = EnemyState.Idle;

    public Transform player;
    public float visionRange = 10f;
    public float visionAngle = 45f;
    public float moveSpeed = 3f;
    public float maxHealth = 100f;

    private float currentHealth;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    void Start()
    {
        currentHealth = maxHealth;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        SetState(EnemyState.Idle);
    }

    void Update()
    {
        if (CurrentState == EnemyState.Dead) return;

        DetectPlayer();

        if (CurrentState == EnemyState.Chasing)
            ChasePlayer();
    }

    void DetectPlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer < visionRange)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer < visionAngle / 2f)
            {
                SetState(EnemyState.Chasing);
                return;
            }
        }

        SetState(EnemyState.Idle);
    }

    void ChasePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    public void TakeDamage(float damage)
    {
        if (CurrentState == EnemyState.Dead) return;

        currentHealth -= damage;
        Debug.Log("Enemy state: damage");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        SetState(EnemyState.Dead);
        Debug.Log("Enemy Dead");
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        currentHealth = maxHealth;
        SetState(EnemyState.Idle);
        gameObject.SetActive(true);
    }

    private void SetState(EnemyState newState)
    {
        if (newState == CurrentState) return;

        CurrentState = newState;
        Debug.Log("Enemy State: " + CurrentState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * visionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * visionRange);
    }
}
