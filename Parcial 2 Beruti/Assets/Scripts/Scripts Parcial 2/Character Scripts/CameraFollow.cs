using UnityEngine;

public class CameraFollowCollision : MonoBehaviour
{
    [Header("Objetivo (pivot)")]
    public Transform target; // Asigná aquí el CameraPivot

    [Header("Distancia y suavizado")]
    public float distance = 3f;
    public float minDistance = 0.3f;
    public float maxDistance = 3f;
    public float smoothSpeed = 10f;

    [Header("Colisión")]
    public LayerMask collisionMask;
    public float collisionBuffer = 0.2f;

    [Header("Rotación con mouse")]
    public float mouseSensitivity = 2f;
    public float minPitch = -60f;
    public float maxPitch = 60f;

    private float pitch = 0f;
    private Vector3 smoothVelocity = Vector3.zero;
    private float currentDistance;

    void Start()
    {
        currentDistance = distance;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Input del mouse (vertical)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Calcular rotación combinando el giro del jugador y el pitch de la cámara
        Quaternion rotation = Quaternion.Euler(pitch, target.eulerAngles.y, 0);

        // Dirección hacia atrás
        Vector3 desiredDir = rotation * Vector3.back;

        // Posición base del pivot (que ya está a la derecha del jugador)
        Vector3 pivotPos = target.position;

        // Colisión
        if (Physics.SphereCast(pivotPos, 0.2f, desiredDir, out RaycastHit hit, distance, collisionMask))
        {
            currentDistance = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, maxDistance);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, distance, Time.deltaTime * smoothSpeed);
        }

        // Aplicar posición
        Vector3 finalPos = pivotPos + desiredDir * currentDistance;
        transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref smoothVelocity, 0.05f);

        // Mirar hacia el pivot
        transform.LookAt(pivotPos);
    }
}
