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
    public float tolleranzaX = 0.2f;

    [Header("Combattimento")]
    public int damage = 1;

    [DoNotSerialize] public Vector3 startPos;

    private Transform playerTransform;
    private PlayerLightController playerLightController;
    private SpriteRenderer spriteRenderer;

    private int currentDirection = 1;
    private float traveledDistance = 0f;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        playerLightController = FindFirstObjectByType<PlayerLightController>();
    }

    public void AggiornaRiferimentoPlayer(Transform nuovoPlayer)
    {
        playerTransform = nuovoPlayer;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        bool isPlayerLightOn = (playerLightController != null && playerLightController.IsLightOn);

        if (isPlayerLightOn && playerTransform != null)
        {
            // --- INSEGUIMENTO (LUCE ACCESA) ---
            float distanzaDizionaleX = playerTransform.position.x - transform.position.x;

            if (Mathf.Abs(distanzaDizionaleX) > tolleranzaX)
            {
                float directionX = Mathf.Sign(distanzaDizionaleX);
                transform.position += new Vector3(directionX * chaseSpeed * Time.deltaTime, 0, 0);

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (directionX < 0);
                    currentDirection = directionX > 0 ? 1 : -1;
                }
            }
            else
            {
                if (spriteRenderer != null)
                {
                    float direzioneSguardo = Mathf.Sign(distanzaDizionaleX);
                    if (Mathf.Abs(distanzaDizionaleX) > 0.01f)
                        spriteRenderer.flipX = (direzioneSguardo < 0);
                }
            }

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
                spriteRenderer.flipX = (currentDirection < 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            Vector3 deathPosition = player.transform.position;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.deathCount += damage;
                GameManager.Instance.RespawnPlayer(deathPosition);
            }

            player.Die();
        }
    }
}