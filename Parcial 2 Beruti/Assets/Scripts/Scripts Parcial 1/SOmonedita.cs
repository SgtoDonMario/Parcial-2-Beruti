using UnityEngine;

[CreateAssetMenu(fileName = "NewCoin", menuName = "Collectibles/Coin")]
public class CoinData : ScriptableObject
{
    [Header("Coin Settings")]
    public string coinName = "Default Coin";
    public int coinValue = 1;

    [Header("Visual")]
    public Color coinColor = Color.yellow;
}