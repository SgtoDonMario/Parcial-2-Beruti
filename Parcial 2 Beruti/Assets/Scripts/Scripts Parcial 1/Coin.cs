using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Coin : MonoBehaviour
{
    public CoinData coinData;

    private void Start()
    {
        // Configurar color si tiene Renderer
        Renderer r = GetComponent<Renderer>();
        if (r != null && coinData != null)
            r.material.color = coinData.coinColor;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador la toca
        if (other.CompareTag("Player") && coinData != null)
        {
            Debug.Log($"Moneda recogida: {coinData.coinName} (+{coinData.coinValue})");

            // Acá podrías sumar el valor al inventario o contador global
            // Ejemplo: GameManager.Instance.AddCoins(coinData.coinValue);

            Destroy(gameObject); // eliminar la moneda del mapa
        }
    }
}