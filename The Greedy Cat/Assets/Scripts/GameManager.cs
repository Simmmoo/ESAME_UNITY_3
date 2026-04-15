using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;
    public PlayerController player;
    public float respawnDelay;

    [Header("Snacks")]
    public int SnackPoint;
    public TextMeshProUGUI MySnacksText;

    public GameObject myCinemachine;

    [Header("Death Count")]
    public  int deathCount = 0; // Conta le morti del player
    public Image[] lifeIcons; // Array di immagini per le vite


    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Effects")]
    public GameObject deathParticlesPrefab;

    [Header("Audio")]
    [Header("Audio")] 
    public AudioSource musicSource;
    private AudioMusic audioMusic;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioMusic = FindObjectOfType<AudioMusic>();
    }

    private void Update()
    {
         MySnacksText.text = SnackPoint.ToString() + "/4";
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        respawnPoint = newCheckpoint; // Aggiorna l'ultimo checkpoint raggiunto
    }

    public void RespawnPlayer()
    {
        if (deathCount < 9)  
        {
            ReduceLifeOpacity();

            if (deathParticlesPrefab != null && player != null)
            {
                Instantiate(deathParticlesPrefab, player.transform.position, Quaternion.identity);
            }
            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            gameOverPanel.SetActive(true);
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        GameObject newPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        player = newPlayer.GetComponent<PlayerController>();

        myCinemachine.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
    }

    private void ReduceLifeOpacity()
    {
        if (deathCount <= lifeIcons.Length || deathCount >= lifeIcons.Length)
            { 

            for (int i = lifeIcons.Length - 1; i > lifeIcons.Length - 1- deathCount; i--) {
                lifeIcons[i].color = new Color (1,1,1,0.2f) ;
            
            }
        }
    
}
public void AddPoints()
{ 
    {
        SnackPoint++;
    }
}

    public void CheckVictory()
    {
        if (SnackPoint >= 4) 
        {
            Debug.Log("Hai vinto");
            victoryPanel.SetActive(true); 
        }
    }
}
