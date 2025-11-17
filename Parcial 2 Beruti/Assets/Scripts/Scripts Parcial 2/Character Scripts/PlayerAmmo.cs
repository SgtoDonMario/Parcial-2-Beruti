using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerWeapon weapon;             // El script de tu arma
    public TextMeshProUGUI ammoText;        // Texto de munición

    void Update()
    {
        if (weapon != null && ammoText != null)
        {
            ammoText.text = weapon.currentAmmo + "/" + weapon.extraMagazines;
        }
    }

}
