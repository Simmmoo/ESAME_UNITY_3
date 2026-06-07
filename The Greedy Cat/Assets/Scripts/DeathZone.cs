using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {

            player.Die();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.deathCount++;
                GameManager.Instance.RespawnPlayer();
            }
        }
    }
}


