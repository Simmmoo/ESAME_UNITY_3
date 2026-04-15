using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSnacks : MonoBehaviour
{
    private GameManager gameManager;

    [Header("Effects")]
    public GameObject SnackParticlesPrefab;

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            // Aggiungi punti
            gameManager.AddPoints();

            if (SnackParticlesPrefab != null)
            {
                GameObject particles = Instantiate(SnackParticlesPrefab, transform.position, Quaternion.identity);
            }

            // Disattiva lo snack
            gameObject.SetActive(false);
        }
    }

}
