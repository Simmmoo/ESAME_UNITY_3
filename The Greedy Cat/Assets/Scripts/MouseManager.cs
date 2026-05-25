using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [Header("Movimento Base (Luce Spenta)")]
    public float speed = 2f;        // Velocità del movimento di pattuglia
    public float distance = 3f;     // Distanza massima che può percorrere prima di girarsi

    [Header("Inseguimento (Luce Accesa)")]
    public float chaseSpeed = 4f;   // Il topo corre più veloce quando insegue

    [Header("Combattimento")]
    public int damage = 1;          // Danni inflitti

    [DoNotSerialize] public Vector3 startPos;

    private Transform playerTransform;
    private PlayerLightController playerLightController;
    private SpriteRenderer spriteRenderer;

    // Variabili interne per calcolare la nuova pattuglia dinamica
    private int currentDirection = 1; // 1 = Destra, -1 = Sinistra
    private float traveledDistance = 0f;

    void Start()
    {
        startPos = transform.position; // Posizione iniziale assoluta al primo spawn
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Cerca il gatto nella scena tramite il Tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerLightController = playerObj.GetComponent<PlayerLightController>();
        }
    }

    void Update()
    {
        // Controlliamo se la luce del gatto è accesa
        bool isPlayerLightOn = (playerLightController != null && playerLightController.IsLightOn);

        if (isPlayerLightOn && playerTransform != null)
        {
            // --- INSEGUIMENTO (LUCE ACCESA) ---
            float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);

            // Muove il topo verso il player
            transform.position += new Vector3(directionX * chaseSpeed * Time.deltaTime, 0, 0);

            // Quando insegue, resettiamo la distanza percorsa in modo che, al momento dello spegnimento,
            // il punto in cui si ferma diventi il NUOVO centro del suo movimento di pattuglia
            startPos = transform.position;
            traveledDistance = 0f;

            // Se va a destra (directionX > 0) si muove in un senso, a sinistra (directionX < 0) si flippa lo sprite
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (directionX < 0);
                // Aggiorna anche la direzione interna in modo che riparta coerente allo spegnimento
                currentDirection = directionX > 0 ? 1 : -1;
            }
        }
        else
        {
            // --- PATTUGLIAMENTO FLUIDO (LUCE SPENTA) ---
            // Calcola lo spostamento di questo frame
            float movement = currentDirection * speed * Time.deltaTime;
            transform.position += new Vector3(movement, 0, 0);

            // Accumula la distanza percorsa (usiamo il valore assoluto)
            traveledDistance += Mathf.Abs(movement);

            // Se il topo ha percorso tutta la distanza impostata dall'Inspector, inverte la marcia
            if (traveledDistance >= distance)
            {
                currentDirection = -currentDirection; // Inverte (da 1 a -1 o viceversa)
                traveledDistance = 0f;               // Resetta il contatore per la nuova direzione
            }

            // Gira lo sprite in base alla direzione attuale di pattugliamento
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentDirection < 0);
            }
        }
    }

    // Collisione identica a prima
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            player.Die();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.deathCount += damage;
                GameManager.Instance.RespawnPlayer();
            }
        }
    }
}