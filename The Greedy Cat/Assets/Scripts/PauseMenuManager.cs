using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pannelli UI (Persistenti)")]
    [SerializeField] private CanvasGroup pauseMenuCanvasGrp;
    [SerializeField] private CanvasGroup settingsMenuCanvasGrp;
    [SerializeField] private CanvasGroup startScreenCanvasGrp;

    [Header("Riferimento Player")]
    private GameObject player;

    private bool isPaused = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        // Al via nascondiamo tutto in modo pulito e sicuro
        ChangeCanvasGroupState(pauseMenuCanvasGrp, false);
    }

    public void TogglePause()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (player == null || !player.activeInHierarchy)
        {
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f; // Congela il gioco
            ChangeCanvasGroupState(pauseMenuCanvasGrp, true);
            ChangeCanvasGroupState(settingsMenuCanvasGrp, false); // Sicurezza
        }
        else
        {
            ResumeGame();
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Ripristina il tempo
        ChangeCanvasGroupState(pauseMenuCanvasGrp, false);
        ChangeCanvasGroupState(settingsMenuCanvasGrp, false);
    }

    // TASTO: SETTINGS (Spegne totalmente la pausa per evitare blocchi di click)
    public void OpenSettingsFromPause()
    {
        if (settingsMenuCanvasGrp != null)
        {
            // Spegniamo la pausa del tutto (Interattività e Raycast inclusi)
            ChangeCanvasGroupState(pauseMenuCanvasGrp, false);

            // Accendiamo i Settings
            ChangeCanvasGroupState(settingsMenuCanvasGrp, true);
        }
        else
        {
            Debug.LogError("PauseMenuManager: Manca il riferimento a Settings Menu Canvas Grp!");
        }
    }

    // TASTO INDIETRO DEI SETTINGS
    public void CloseSettingsAndReturnToPause()
    {
        if (isPaused)
        {
            // Se siamo in partita: spegne i settings e riaccende la pausa
            ChangeCanvasGroupState(settingsMenuCanvasGrp, false);
            ChangeCanvasGroupState(pauseMenuCanvasGrp, true);
        }
        else
        {
            // Se eravamo nel menu iniziale: spegne i settings e riaccende lo start menu
            ChangeCanvasGroupState(settingsMenuCanvasGrp, false);
            if (startScreenCanvasGrp != null)
            {
                ChangeCanvasGroupState(startScreenCanvasGrp, true);
            }
        }
    }

    public void OpenLevelSelectionFromPause()
    {
        Debug.Log("Apertura menu Selezione Livelli...");
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        int currentLevelIndex = 0;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex != 0)
            {
                currentLevelIndex = scene.buildIndex;
                break;
            }
        }

        if (currentLevelIndex != 0)
        {
            SceneManager.UnloadSceneAsync(currentLevelIndex);
        }

        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) player.SetActive(false);

        ChangeCanvasGroupState(pauseMenuCanvasGrp, false);
        ChangeCanvasGroupState(settingsMenuCanvasGrp, false);

        if (startScreenCanvasGrp != null)
        {
            ChangeCanvasGroupState(startScreenCanvasGrp, true);
        }

        Scene startMenuScene = SceneManager.GetSceneByName("START_MENU");
        if (startMenuScene.IsValid())
        {
            SceneManager.SetActiveScene(startMenuScene);
        }
    }

    // Gestione blindata degli stati fisici del CanvasGroup
    private void ChangeCanvasGroupState(CanvasGroup cg, bool visible)
    {
        if (cg != null)
        {
            cg.alpha = visible ? 1f : 0f;
            cg.blocksRaycasts = visible; // Se false, i click passano attraverso senza bloccarsi
            cg.interactable = visible;   // Se false, i bottoni interni non rispondono
        }
    }
}