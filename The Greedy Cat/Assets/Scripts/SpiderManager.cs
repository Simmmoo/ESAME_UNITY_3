using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpiderManager : MonoBehaviour
{
    public float speed;     // Velocità del movimento
    public float distance;  // Distanza da percorrere verso il basso
    public int damage; // Danni inflitti al player

    [DoNotSerialize] public Vector3 startPos;

    public bool GoesDown = false;

    void Start()
    {
        startPos = transform.position; // Memorizza la posizione iniziale
    }

    void Update()
    {
        // Movimento su-giù con PingPong (oscilla tra 0 e distance)
        if (GoesDown)
        {
            float newY = startPos.y - Mathf.PingPong(Time.time * speed, distance);
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }
        else
        {
            float newY = startPos.y + Mathf.PingPong(Time.time * speed, distance);
            transform.position = new Vector3(startPos.x, newY, startPos.z);

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
