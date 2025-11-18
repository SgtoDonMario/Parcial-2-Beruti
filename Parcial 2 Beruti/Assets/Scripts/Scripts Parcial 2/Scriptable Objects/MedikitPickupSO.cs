using UnityEngine;

[CreateAssetMenu(fileName = "NewMedikitPickup", menuName = "Pickups/Medikit")]
public class MedikitPickupSO : ScriptableObject
{
    public int healAmount = 30; // +30HP
}
