using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Playables;

public class ChangeScene : MonoBehaviour
{
    public static ChangeScene Instance;

    [Header("Impostazioni Fade (Dissolvenza)")]
    public float fadeDuration = 1.2f;
    public CanvasGroup startScreenCanvasGrp;
    public Image fadePanel;

    [Header("Cinematica")]
    [SerializeField] private PlayableDirector cinematicDirector;

    [SerializeField] GameObject player;
    [SerializeField] Transform[] startPositions;
    [SerializeField] GameManager myGameManager;

    [Header("Cinematica Fine Gioco")]
    [SerializeField] private PlayableDirector endCinematicDirector;
    [SerializeField] private GameObject pnlGame;

    private bool skipCinematic = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        Debug.Log("CHIAMATA ON ENABLE IN CHANGE SCENE");
        myGameManager.evt_PlayerDied.AddListener(ReassignPlayer);
        myGameManager.evt_gameOver.AddListener(ResetScenes);
        myGameManager.evt_Victory.AddListener(StartEndCinematic); // <-- aggiunto
    }

    private void OnDisable()
    {
        myGameManager.evt_PlayerDied.RemoveListener(ReassignPlayer);
        myGameManager.evt_gameOver.RemoveListener(ResetScenes);
        myGameManager.evt_Victory.RemoveListener(StartEndCinematic); // <-- aggiunto
    }

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0, 0, 0, 0);
            fadePanel.gameObject.SetActive(false);
        }
    }

    #region Fade helpers

    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;
        Color c = fadePanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
        c.a = 1;
        fadePanel.color = c;
    }

    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;
        Color c = fadePanel.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = 1 - Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }
        c.a = 0;
        fadePanel.color = c;
        fadePanel.gameObject.SetActive(false);
    }

    // Il fadePanel e' figlio di pnlGame: per poterlo mostrare pnlGame deve essere attivo.
    // Nasconde pero' esplicitamente tutto il resto (HUD, pulsanti di movimento) cosi'
    // da non farli comparire mentre siamo fuori dal gameplay vero e proprio.
    private void SetGameHudVisible(bool visible)
    {
        if (pnlGame == null) return;
        foreach (Transform child in pnlGame.transform)
        {
            if (fadePanel != null && child == fadePanel.transform) continue;
            child.gameObject.SetActive(visible);
        }
    }

    // Blocca il gatto (niente input, niente fisica, animazione in idle) per tutta la durata del fade
    private void FreezePlayer()
    {
        PlayerController pc = myGameManager != null ? myGameManager.player : null;
        if (pc == null) return;

        pc.Freeze();
    }

    private void UnfreezePlayer()
    {
        PlayerController pc = myGameManager != null ? myGameManager.player : null;
        if (pc == null) return;

        pc.Unfreeze();
    }

    // La virtual camera ha uno smorzamento sul Follow: se il player viene teletrasportato
    // (cambio livello/selezione dal menu) la camera ci mette un momento a raggiungerlo,
    // lasciando il resto del livello fuori inquadratura durante il fade. Qui la agganciamo
    // subito alla nuova posizione, senza scivolamento.
    private void SnapCameraToPlayer(Vector3 previousPosition)
    {
        if (myGameManager == null || myGameManager.myCinemachine == null || player == null) return;

        CinemachineVirtualCamera vcam = myGameManager.myCinemachine.GetComponent<CinemachineVirtualCamera>();
        if (vcam == null) return;

        vcam.Follow = player.transform;
        vcam.OnTargetObjectWarped(player.transform, player.transform.position - previousPosition);
    }

    #endregion

    void ReassignPlayer()
    {
        player = myGameManager.player.gameObject;
        player.SetActive(true);
    }

    public void ResetScenes()
    {
        UnloadGame();
        currentScene = "";
    }

    public void StartLevelTransition()
    {
        StartCoroutine(FadeAndLoadNextLevel());
    }

    private IEnumerator FadeAndLoadNextLevel()
    {
        FreezePlayer();

        yield return StartCoroutine(FadeOut());

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int nextLevelIndex = currentLevelIndex + 1;
        int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);

        if (nextLevelIndex > highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestLevelUnlocked", nextLevelIndex);
            PlayerPrefs.Save();
        }

        if (nextLevelIndex < SceneManager.sceneCountInBuildSettings)
        {
            yield return SceneManager.LoadSceneAsync(nextLevelIndex, LoadSceneMode.Additive);
        }
        else
        {
            SceneManager.LoadScene("START_MENU");
        }

        yield return StartCoroutine(FadeIn());

        UnfreezePlayer();
    }

    string currentScene = "";

    // Passaggio livello in gioco (es. LevelTransitionTrigger): fade to black, cambio scena, fade in.
    public void FadeToScene(string currentSceneName, string nextSceneName)
    {
        StartCoroutine(FadeToSceneCoroutine(currentSceneName, nextSceneName));
    }

    private IEnumerator FadeToSceneCoroutine(string currentSceneNameToUnload, string nextSceneNameToLoad)
    {
        // Se il gatto e' a mezz'aria (es. in salto), aspetta che atterri prima di bloccarlo e far partire il fade
        PlayerController pc = myGameManager != null ? myGameManager.player : null;
        if (pc != null)
        {
            float waitedTime = 0f;
            const float maxLandingWait = 2f;
            while (!pc.isGrounded && waitedTime < maxLandingWait)
            {
                waitedTime += Time.deltaTime;
                yield return null;
            }
        }

        FreezePlayer();

        yield return StartCoroutine(FadeOut());

        if (!string.IsNullOrEmpty(nextSceneNameToLoad))
            yield return SceneManager.LoadSceneAsync(nextSceneNameToLoad, LoadSceneMode.Additive);

        if (!string.IsNullOrEmpty(currentSceneNameToUnload))
            yield return SceneManager.UnloadSceneAsync(currentSceneNameToUnload);

        currentScene = nextSceneNameToLoad;

        yield return StartCoroutine(FadeIn());

        UnfreezePlayer();
    }

    public void LoadGame()
    {
        StartCoroutine(PlayCinematicThenLoad());
    }

    private IEnumerator PlayCinematicThenLoad()
    {
        skipCinematic = false;

        // pnlGame deve essere attivo perche' il fadePanel (suo figlio) sia visibile,
        // ma la HUD/i pulsanti di movimento restano nascosti finche' il gameplay non e' pronto
        if (pnlGame != null)
        {
            pnlGame.SetActive(true);
            SetGameHudVisible(false);
        }

        // Fade to black: copre il passaggio dalla start screen alla cinematica
        yield return StartCoroutine(FadeOut());

        // Nascondi la start screen
        startScreenCanvasGrp.alpha = 0;
        startScreenCanvasGrp.blocksRaycasts = false;

        // Attiva e avvia la cinematica
        if (cinematicDirector != null)
        {
            cinematicDirector.gameObject.SetActive(true);

            bool cinematicFinished = false;
            cinematicDirector.stopped += _ => {
                Debug.Log("STOPPED EVENT FIRED");
                cinematicFinished = true;
            };

            cinematicDirector.Play();
            Debug.Log("Cinematic started, state: " + cinematicDirector.state);

            // Rivela la cinematica
            yield return StartCoroutine(FadeIn());

            yield return new WaitUntil(() => {
                Debug.Log("Waiting... state: " + cinematicDirector.state + " | finished: " + cinematicFinished + " | skip: " + skipCinematic);
                return cinematicFinished || skipCinematic;
            });

            Debug.Log("Uscito dal WaitUntil");

            if (skipCinematic)
                cinematicDirector.Stop();

            // Fade to black: copre la fine della cinematica
            yield return StartCoroutine(FadeOut());

            // Disattiva tutto il GameObject della cinematica (figli compresi)
            cinematicDirector.gameObject.SetActive(false);
        }

        // Prepara il player e il livello, ancora nascosti dietro al nero
        player = myGameManager.player.gameObject;
        Vector3 previousPlayerPos = player.transform.position;
        player.transform.position = startPositions[0].position;
        player.SetActive(true);
        FreezePlayer();
        SnapCameraToPlayer(previousPlayerPos);

        if (pnlGame != null)
        {
            pnlGame.SetActive(true);
            SetGameHudVisible(true);
        }

        yield return SceneManager.LoadSceneAsync("LVL_01", LoadSceneMode.Additive);
        currentScene = "LVL_01";

        // Rivela il gameplay
        yield return StartCoroutine(FadeIn());

        UnfreezePlayer();
    }

    public void SkipCinematic()
    {
        skipCinematic = true;
    }

    public void UnloadGame()
    {
        if (currentScene != "") SceneManager.UnloadSceneAsync(currentScene);
    }

    public void LoadSpecificLevel(string levelName)
    {
        if (pnlGame != null)
        {
            pnlGame.SetActive(true);
            SetGameHudVisible(true);
        }

        UnloadGame();
        currentScene = levelName;

        Vector3 previousPlayerPos = player.transform.position;
        player.transform.position = startPositions[int.Parse(currentScene.Split('0')[1]) - 1].position;
        player.SetActive(true);
        SnapCameraToPlayer(previousPlayerPos);

        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
    }

    public void StartEndCinematic()
    {
        StartCoroutine(PlayEndCinematicThenMenu());
    }

    private IEnumerator PlayEndCinematicThenMenu()
    {
        FreezePlayer();

        // pnlGame attivo solo per mostrare il fadePanel: la HUD di gioco resta nascosta,
        // non dobbiamo piu' tornare al gameplay da qui
        if (pnlGame != null)
        {
            pnlGame.SetActive(true);
            SetGameHudVisible(false);
        }

        // Fade to black: copre il taglio tra il gameplay e la cinematica finale
        yield return StartCoroutine(FadeOut());

        // Scarica il livello e disattiva il player
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != "START_MENU" && scene.isLoaded)
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
        currentScene = "";

        if (player != null)
            player.SetActive(false);

        // Piccola attesa per dare tempo allo scaricamento
        yield return new WaitForSeconds(0.1f);

        if (endCinematicDirector != null)
        {
            endCinematicDirector.gameObject.SetActive(true);

            bool finished = false;
            endCinematicDirector.stopped += _ => finished = true;
            endCinematicDirector.Play();

            // Rivela la cinematica finale
            yield return StartCoroutine(FadeIn());

            yield return new WaitUntil(() => finished);

            // Fade to black: copre la fine della cinematica
            yield return StartCoroutine(FadeOut());

            endCinematicDirector.gameObject.SetActive(false);
        }

        // Riattiva la start screen (nascosta dietro al nero)
        if (startScreenCanvasGrp != null)
        {
            startScreenCanvasGrp.gameObject.SetActive(true);
            startScreenCanvasGrp.alpha = 1;
            startScreenCanvasGrp.blocksRaycasts = true;
        }

        // Rivela la start screen
        yield return StartCoroutine(FadeIn());

        // La partita e' finita: nascondi del tutto pnlGame (HUD + pulsanti di movimento)
        if (pnlGame != null)
            pnlGame.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("START_MENU");
    }
}
