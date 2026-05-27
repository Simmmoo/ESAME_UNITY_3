using UnityEngine;

public class DogHouse : MonoBehaviour
{
    private FireworksManager fireworksManager;
    private SpriteRenderer playerSprite;

    void Start()
    {
        fireworksManager = FindFirstObjectByType<FireworksManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Gatto entrato nella cuccia. Al sicuro!");
            if (fireworksManager != null) fireworksManager.SetPlayerSafe(true);

            // PROTOTIPO VISIVO: Nascondiamo il gatto per far capire che è dentro
            playerSprite = collision.GetComponentInChildren<SpriteRenderer>();
            if (playerSprite != null) playerSprite.color = new Color(1, 1, 1, 0.3f); // Diventa semi-trasparente
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Gatto uscito dalla cuccia. Pericolo!");
            if (fireworksManager != null) fireworksManager.SetPlayerSafe(false);

            // PROTOTIPO VISIVO: Il gatto torna visibile
            if (playerSprite != null) playerSprite.color = new Color(1, 1, 1, 1f);
        }
    }
}