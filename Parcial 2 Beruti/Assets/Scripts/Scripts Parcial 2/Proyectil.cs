using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 40f;
    public float lifetime = 3f;
    public float damage = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(50f);
        }

        SecurityCamera cam = collision.gameObject.GetComponent<SecurityCamera>();
        if (cam != null)
        {
            cam.TakeDamage(damage);
        }


        Destroy(gameObject);

    }
}
