using UnityEngine;

public class GameManager : MonoBehaviour
{
    public EnemyAI enemy; // arrastrá el Enemy de la escena al inspector

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (enemy != null && !enemy.gameObject.activeSelf)
            {
                enemy.Respawn();
            }
        }
    }
}