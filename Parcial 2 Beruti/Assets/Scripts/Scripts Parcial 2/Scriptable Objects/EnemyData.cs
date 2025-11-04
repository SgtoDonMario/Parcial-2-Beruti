using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Atributos Base")]
    public float maxHealth = 100f;
    public float visionDistance = 10f;
    public float visionAngle = 60f;
    public float moveSpeed = 7f;

    [Header("Daño")]
    public float damage = 10f;
}
