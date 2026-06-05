using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] public GameObject playerObject;
    public PlayerController player;
    public float respawnDelay;

    [Header("Snacks")]
    public int SnackPoint;
    public TextMeshProUGUI MySnacksText;
    public GameObject myCinemachine;

    [Header("Death Count")]
    public int deathCount = 0;
    public Image[] lifeIcons;

    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Effects")]
    public GameObject deathParticlesPrefab;

    [Header("Audio")]
    public AudioSource musicSource;
    private AudioMusic audioMusic;

    [System.Obsolete]
    public void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioMusic = Object.FindFirstObjectByType<AudioMusic>();
    }

    private void Update()
    {
        MySnacksText.text = SnackPoint.ToString() + "/4";
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        respawnPoint = newCheckpoint;
    }

    public void RespawnPlayer(Vector3 deathPosition)
    {
        if (deathCount < 9)
        {
            ReduceLifeOpacity();

            if (deathParticlesPrefab != null)
                Instantiate(deathParticlesPrefab, deathPosition, Quaternion.identity);

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

        if (respawnPoint == null)
        {
            SpawnPoint puntoDiSpawn = FindFirstObjectByType<SpawnPoint>();
            if (puntoDiSpawn != null)
                respawnPoint = puntoDiSpawn.transform;
            else
            {
                Debug.LogWarning("RespawnCoroutine: nessun SpawnPoint trovato in scena!");
                yield break;
            }
        }

        GameObject newPlayer = Instantiate(playerPrefab, respawnPoint.position, Quaternion.identity);
        player = newPlayer.GetComponent<PlayerController>();
        myCinemachine.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;

        // Aggiorna riferimento player nel FireworksManager se presente
        FireworksManager fireworks = FindFirstObjectByType<FireworksManager>();
        if (fireworks != null)
            fireworks.AggiornaRiferimentoPlayer(player);

        // Aggiorna riferimento player nel MouseManager se presente
        MouseManager mouse = FindFirstObjectByType<MouseManager>();
        if (mouse != null)
            mouse.AggiornaRiferimentoPlayer(player.transform);

        PlayerLightController lightController = FindFirstObjectByType<PlayerLightController>();
        if (lightController != null)
            lightController.AggiornaLuceSuPlayer();


    }

    private void ReduceLifeOpacity()
    {
        if (deathCount <= lifeIcons.Length || deathCount >= lifeIcons.Length)
        {
            for (int i = lifeIcons.Length - 1; i > lifeIcons.Length - 1 - deathCount; i--)
            {
                lifeIcons[i].color = new Color(1, 1, 1, 0.2f);
            }
        }
    }

    // --- LOGICA SELEZIONE LIVELLI ---

    public void SelezionaLivello1()
    {
        CaricaLivelloSelezionato("LVL_01", 1);
    }

    public void SelezionaLivello2()
    {
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        if (highestUnlocked >= 2)
            CaricaLivelloSelezionato("LVL_02", 2);
        else
            Debug.Log("LVL_02 non ancora sbloccato!");
    }

    public void SelezionaLivello3()
    {
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
        if (highestUnlocked >= 3)
            CaricaLivelloSelezionato("LVL_03", 3);
        else
            Debug.Log("LVL_03 non ancora sbloccato!");
    }

    private void CaricaLivelloSelezionato(string nomeLivello, int numeroLivello)
    {
        Time.timeScale = 1f;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scenaCorrente = SceneManager.GetSceneAt(i);
            if (scenaCorrente.name != "START_MENU")
                SceneManager.UnloadSceneAsync(scenaCorrente.buildIndex);
        }

        StartCoroutine(CaricaESpawna(nomeLivello));
    }

    private IEnumerator CaricaESpawna(string nomeLivello)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nomeLivello, LoadSceneMode.Additive);
        yield return new WaitUntil(() => op.isDone);
        yield return null;

        // Riattiva il player se è disattivato
        if (playerObject != null && !playerObject.activeSelf)
            playerObject.SetActive(true);

        SpawnPoint puntoDiSpawn = FindFirstObjectByType<SpawnPoint>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null && puntoDiSpawn != null)
        {
            playerObj.transform.position = puntoDiSpawn.transform.position;

            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            player = playerObj.GetComponent<PlayerController>();
            respawnPoint = puntoDiSpawn.transform;

            Debug.Log("Player spawnato in: " + nomeLivello);
        }
        else
        {
            if (playerObj == null) Debug.LogWarning("Player non trovato.");
            if (puntoDiSpawn == null) Debug.LogWarning("SpawnPoint non trovato in: " + nomeLivello);
        }
    }

    public void AddPoints()
    {
        SnackPoint++;
    }

    public void CheckVictory()
    {
        if (SnackPoint >= 4)
        {
            Debug.Log("Hai vinto");
            if (victoryPanel.GetComponentInParent<CanvasGroup>() != null)
                victoryPanel.GetComponentInParent<CanvasGroup>().alpha = 1;
            victoryPanel.SetActive(true);
            Debug.Log("Si attiva il panel");
        }
    }

    // --- LOGICA PAUSA ---

    public void Pausa()
    {
        Time.timeScale = 0f;
        Debug.Log("Gioco in Pausa");
    }

    public void SbloccaGioco()
    {
        Time.timeScale = 1f;
        Debug.Log("Gioco Ripreso");
    }

    public void TornaAlMenu()
    {
        Time.timeScale = 1f;

        // Distruggi il player istanziato se è diverso dal playerObject originale
        if (player != null && player.gameObject != playerObject)
            Destroy(player.gameObject);

        // Disattiva il player originale nella Hierarchy
        if (playerObject != null)
            playerObject.SetActive(false);

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scena = SceneManager.GetSceneAt(i);
            if (scena.name != "START_MENU")
                SceneManager.UnloadSceneAsync(scena.buildIndex);
        }

        // Resetta il riferimento al player
        player = null;
        respawnPoint = null;

        ChangeScene changeScene = FindFirstObjectByType<ChangeScene>();
        if (changeScene != null)
        {
            changeScene.startScreenCanvasGrp.alpha = 1f;
            changeScene.startScreenCanvasGrp.blocksRaycasts = true;
            changeScene.fadePanel.gameObject.SetActive(true);
            changeScene.fadePanel.color = new Color(0, 0, 0, 0);
        }
    }
}