using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Ajustes")]
    public float speed = 40f;
    public float lifetime = 3f;
    public float damage = 10f;

    [Header("Opcional - Quién disparó")]
    public GameObject owner; // Jugador o enemigo

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // El proyectil viaja directamente hacia adelante
        rb.linearVelocity = transform.forward * speed;

        // Se autodestruye
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar colisiones con quien lo disparó
        if (other.gameObject == owner) return;

        // Daño a enemigos
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Daño al jugador
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Daño a cámaras u otros objetos
        SecurityCamera cam = other.GetComponent<SecurityCamera>();
        if (cam != null)
        {
            cam.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Si pega en otra cosa (paredes, suelo)
        Destroy(gameObject);
    }
}