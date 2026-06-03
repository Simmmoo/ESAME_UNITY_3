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
    public PlayerController player;
    public float respawnDelay;

    [Header("Snacks")]
    public int SnackPoint;
    public TextMeshProUGUI MySnacksText;
    public GameObject myCinemachine;

    [Header("Death Count")]
    public int deathCount = 0; // Conta le morti del player
    public Image[] lifeIcons; // Array di immagini per le vite


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

        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        audioMusic = Object.FindFirstObjectByType<AudioMusic>();
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
            for (int i = lifeIcons.Length - 1; i > lifeIcons.Length - 1 - deathCount; i--)
            {
                lifeIcons[i].color = new Color(1, 1, 1, 0.2f);
            }
        }
    }



// --- LOGICA SELEZIONE LIVELLI CON SPOSTAMENTO PLAYER ---

// Questa funzione viene attivata dal tasto "LVL 01"
public void SelezionaLivello1()
{
    CaricaLivelloSelezionato("LVL_01");
}

// Questa funzione viene attivata dal tasto "LVL 02"
public void SelezionaLivello2()
{
    CaricaLivelloSelezionato("LVL_02");
}

// Questa funzione viene attivata dal tasto "LVL 03"
public void SelezionaLivello3()
{
    CaricaLivelloSelezionato("LVL_03");
}

// Logica interna che fa il lavoro di pulizia, caricamento e spostamento
private void CaricaLivelloSelezionato(string nomeLivello)
{
    // Ripristina il tempo a 1 nel caso in cui fossimo passati da un menu di pausa
    Time.timeScale = 1f;

    // Svuota e scarica qualsiasi altro livello additivo precedentemente aperto per evitare sovrapposizioni
    for (int i = 0; i < SceneManager.sceneCount; i++)
    {
        Scene scenaCorrente = SceneManager.GetSceneAt(i);
        if (scenaCorrente.name != "START_MENU")
        {
            SceneManager.UnloadSceneAsync(scenaCorrente.buildIndex);
        }
    }

    // Carica la nuova scena del livello scelta in modo Additivo
    SceneManager.LoadScene(nomeLivello, LoadSceneMode.Additive);

    // --- AGGIUNTA: LOGICA DI SPOSTAMENTO SULLO SPAWNPOINT ---

    // Cerchiamo nella scena lo SpawnPoint appena caricato (tramite il componente)
    SpawnPoint puntoDiSpawn = FindFirstObjectByType<SpawnPoint>();

    // Cerchiamo il gatto che hai appena attivato
    GameObject player = GameObject.FindGameObjectWithTag("Player");

    if (player != null && puntoDiSpawn != null)
    {
        // Sposta il gatto nella posizione esatta del nuovo SpawnPoint
        player.transform.position = puntoDiSpawn.transform.position;

        // Reset della fisica per evitare che mantenga velocità o vettori dei livelli precedenti
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("Player respawnato con successo nello SpawnPoint di: " + nomeLivello);
    }
    else
    {
        // Log di controllo se qualcosa dovesse andare storto
        if (player == null) Debug.LogWarning("Selezione Livelli: Player non trovato in scena.");
        if (puntoDiSpawn == null) Debug.LogWarning("Selezione Livelli: Nessuno script SpawnPoint trovato nel livello " + nomeLivello);
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
            if (victoryPanel.GetComponentInParent<CanvasGroup>() != null) victoryPanel.GetComponentInParent<CanvasGroup>().alpha = 1;
            victoryPanel.SetActive(true);
            Debug.Log("Si attiva il panel");
        }
    }

    // --- LOGICA PAUSA ---

    // Mette il gioco in pausa 
    public void Pausa()
    {
        Time.timeScale = 0f;
        Debug.Log("Gioco in Pausa");
    }

    // Fà riprendere il gioco normalmente
    public void SbloccaGioco()
    {
        Time.timeScale = 1f;
        Debug.Log("Gioco Ripreso");
    }

}