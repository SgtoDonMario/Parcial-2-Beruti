using UnityEngine;

[CreateAssetMenu(fileName = "NewSecurityCameraData", menuName = "Enemies/Security Camera Data")]
public class SecurityCameraData : ScriptableObject
{
    [Header("Detección")]
    public float visionDistance = 12f;
    public float visionAngle = 45f;

    [Header("Rotación")]
    public float rotationSpeed = 50f;
    public float leftLimit = -45f;
    public float rightLimit = 45f;
    public float pauseTime = 1f;

    [Header("Alertas")]
    public float alertDelay = 0.2f;
}
