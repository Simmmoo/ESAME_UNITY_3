using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    void Start()
    {
        // Cerca il gatto nella scena usando il Tag "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Sposta il gatto nella posizione esatta di questo oggetto SpawnPoint
            player.transform.position = transform.position;

            // Se il gatto ha un Rigidbody2D, azzeriamo la sua velocità
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            Debug.Log("Change Scene funziona");
        }
        else
        {
            Debug.LogWarning("No Player tag");
        }
    }
}