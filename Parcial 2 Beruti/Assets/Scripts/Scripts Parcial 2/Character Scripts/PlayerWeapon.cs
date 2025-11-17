using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Referencias")]
    public Camera playerCamera;          // Asignar la c�mara principal
    public Transform projectileSpawn;    // Asignar el punto desde donde salen los proyectiles
    public GameObject projectilePrefab;  // Prefab del proyectil

    [Header("Disparo")]
    public float fireRate = 0.2f;
    private float nextFireTime = 0f;

    [Header("Municion")]
    public int magazineSize = 15;
    public int currentAmmo;
    public int extraMagazines = 1;
    public int startMag = 1;

    void Start()
    {
        currentAmmo = magazineSize; // Empieza con cargador lleno
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            TryShoot();
        }

        // Recargar con R
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
        }
    }

    void TryShoot()
    {
        if (currentAmmo <= 0) return;

        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        // Raycast desde el centro de la c�mara
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Vector3 targetPoint;

        // Si el rayo impacta algo, disparamos hacia all�
        if (Physics.Raycast(ray, out hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // Si no impacta nada, apuntamos a un punto lejano
            targetPoint = playerCamera.transform.position + playerCamera.transform.forward * 100f;
        }
                                                                                 

        // Calculamos la direcci�n desde el spawn hacia el punto de impacto
        Vector3 shootDirection = (targetPoint - projectileSpawn.position).normalized;

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 100f, Color.green, 1f); // Rayo desde c�mara
        Debug.DrawRay(projectileSpawn.position, shootDirection * 100f, Color.red, 1f); // Rayo desde el spawn

        // Instanciar proyectil
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawn.position, Quaternion.LookRotation(shootDirection));

        // Si el proyectil tiene Rigidbody, le damos velocidad o fuerza
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = shootDirection * 40f; // o AddForce si prefer�s
    }


    void TryReload()
    {
        if (currentAmmo == magazineSize) return;       // Ya est� lleno
        if (extraMagazines <= 0) return;               // No hay m�s cargadores

        extraMagazines--;
        currentAmmo = magazineSize;                    // Se reemplaza el cargador
    }

    public void ResetAmmo()
    {
        currentAmmo = magazineSize;
        extraMagazines = startMag;
    }
}
