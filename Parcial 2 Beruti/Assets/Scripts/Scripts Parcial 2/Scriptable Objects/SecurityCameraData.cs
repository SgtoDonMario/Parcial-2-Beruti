using UnityEngine;

[CreateAssetMenu(fileName = "SecurityCameraData", menuName = "Game/Security Camera Data")]
public class SecurityCameraData : ScriptableObject
{
    [Header("Vida")]
    public float maxHealth = 50f;

    [Header("Rotación Automática")]
    public float rotationSpeed = 30f;
    public float rotationAngle = 45f;
    public float pauseTime = 1f;

    [Header("Detección")]
    public float visionDistance = 12f;
    public float visionAngle = 60f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("UI")]
    public Vector3 uiOffset = new Vector3(0, 1.5f, 0);
}

