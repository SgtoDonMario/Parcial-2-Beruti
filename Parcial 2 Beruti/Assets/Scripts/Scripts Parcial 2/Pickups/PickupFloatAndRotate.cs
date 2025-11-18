using UnityEngine;

public class PickupFloatAndRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // grados por segundo

    [Header("Floating Settings")]
    public float floatAmplitude = 0.5f; // cuánto sube y baja
    public float floatFrequency = 2f;  // velocidad de oscilación

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Rotación 360°
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // Movimiento de subida y bajada
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

