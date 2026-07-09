using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [Header("Movimento Base (Luce Spenta)")]
    public float speed = 2f;
    public float distance = 3f;

    [Header("Inseguimento (Luce Accesa)")]
    public float chaseSpeed = 4f;
    [Tooltip("Soglia di tolleranza per evitare il tremolio quando il topo e' quasi allineato al gatto")]
    public float tolleranzaX = 0.2f; // Se la distanza X e' minore di 0.2, il topo si stabilizza

    [Header("Combattimento")]
    public int damage = 1;

    [DoNotSerialize] public Vector3 startPos;
    private Vector3 spawnPos;

    private Transform playerTransform;
    private PlayerLightController playerLightController;
    private SpriteRenderer spriteRenderer;

    private int currentDirection = 1;
    private float traveledDistance = 0f;

    private void OnEnable()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().evt_PlayerDied.AddListener(OnPlayerDead);
    }
    private void OnApplicationQuit()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().evt_PlayerDied.RemoveListener(OnPlayerDead);
    }
    void Start()
    {
        startPos = transform.position;
        spawnPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        GameObject GM = GameObject.FindGameObjectWithTag("GameManager");
        if(GM != null) playerLightController = GM.GetComponent<PlayerLightController>();


    }

    void Update()
    {
        bool isPlayerLightOn = (playerLightController != null && playerLightController.IsLightOn);
        if (isPlayerLightOn && playerTransform != null)
        {
            // --- INSEGUIMENTO (LUCE ACCESA) ---

            // Calcoliamo la distanza effettiva tra il topo e il gatto sull'asse X
            float distanzaDizionaleX = playerTransform.position.x - transform.position.x;

            // Se la distanza e' maggiore della tolleranza, il topo si muove
            if (Mathf.Abs(distanzaDizionaleX) > tolleranzaX)
            {
                float directionX = Mathf.Sign(distanzaDizionaleX);

                // Muove il topo
                transform.position += new Vector3(directionX * chaseSpeed * Time.deltaTime, 0, 0);

                // Aggiorna lo sprite basandosi sulla direzione effettiva di movimento
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (directionX < 0);
                    currentDirection = directionX > 0 ? 1 : -1;
                }
            }
            // Se la distanza rientra nella tolleranza, il topo si ferma immobile sotto/sopra il gatto,
            // evitando i micro-scatti ma continuando a guardarlo coerentemente
            else
            {
                if (spriteRenderer != null)
                {
                    float direzioneSguardo = Mathf.Sign(distanzaDizionaleX);
                    // Evita di cambiare flip se siamo praticamente a zero spaccato
                    if (Mathf.Abs(distanzaDizionaleX) > 0.01f)
                    {
                        spriteRenderer.flipX = (direzioneSguardo < 0);
                    }
                }
            }

            // Mantiene aggiornata la posizione di pattuglia dinamica per quando si spegnera' la luce
            startPos = transform.position;
            traveledDistance = 0f;
        }
        else
        {
            // --- PATTUGLIAMENTO FLUIDO (LUCE SPENTA) ---
            float movement = currentDirection * speed * Time.deltaTime;
            transform.position += new Vector3(movement, 0, 0);

            traveledDistance += Mathf.Abs(movement);

            if (traveledDistance >= distance)
            {
                currentDirection = -currentDirection;
                traveledDistance = 0f;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (currentDirection < 0);
            }
        }
    }


    void OnPlayerDead()
    {
        Debug.Log("Ricevuto evento");
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        GameObject GM = GameObject.FindGameObjectWithTag("GameManager");
        if (GM != null) playerLightController = GM.GetComponent<PlayerLightController>();

        // Riporta il topo al suo punto di partenza e resetta il pattugliamento
        transform.position = spawnPos;
        startPos = spawnPos;
        traveledDistance = 0f;
        currentDirection = 1;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
    }

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