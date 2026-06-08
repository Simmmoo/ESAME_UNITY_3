using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    [Header("Impostazioni Fade (Dissolvenza)")]
    public Image fadeCanvasImage;
    public float fadeDuration = 1f;
    public CanvasGroup startScreenCanvasGrp;
    public Image fadePanel;

    [SerializeField] GameObject player;

    [SerializeField] Transform[] startPositions;
    [SerializeField] GameManager myGameManager;

    private void OnEnable()
    {
        Debug.Log("CHIAMATA ON ENABLE IN CHANGE SCENE");
        myGameManager.evt_PlayerDied.AddListener(ReassignPlayer);
        myGameManager.evt_gameOver.AddListener(ResetScenes);
    }

    private void OnDisable()
    {
        myGameManager.evt_PlayerDied.RemoveListener(ReassignPlayer);
        myGameManager.evt_gameOver.RemoveListener(ResetScenes);
    }
    private void Start()
    {
        if (fadeCanvasImage != null)
        {
            fadeCanvasImage.gameObject.SetActive(true);
            fadeCanvasImage.color = new Color(0, 0, 0, 0);
        }
    }
    void ReassignPlayer()
    {
        player = myGameManager.player.gameObject;
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
        if (fadeCanvasImage != null)
        {
            float elapsedTime = 0f;
            Color panelColor = fadeCanvasImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                panelColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
                fadeCanvasImage.color = panelColor;
                yield return null;
            }
        }

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
            SceneManager.LoadScene(nextLevelIndex, LoadSceneMode.Additive);
        }
        else
        {
            SceneManager.LoadScene("START_MENU");
        }
    }

    //public void LoadGame() => SceneManager.LoadScene("LVL_01", LoadSceneMode.Additive);

    string currentScene = "";
    public void LoadGame()
    {
        SceneManager.LoadScene("LVL_01", LoadSceneMode.Additive);
        currentScene = "LVL_01";
        startScreenCanvasGrp.alpha = 0;
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.gameObject.SetActive(false);
        startScreenCanvasGrp.blocksRaycasts = false;
        player.transform.position = startPositions[0].position;
        player.SetActive(true);
    }

    public void UnloadGame()
    {
        if (currentScene != "") SceneManager.UnloadSceneAsync(currentScene);
    }

    public void LoadSpecificLevel(string levelName)
    {
        UnloadGame();
        currentScene = levelName;

        player.transform.position = startPositions[int.Parse(currentScene.Split('0')[1]) - 1].position;

        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);

    }
    public void QuitGame() { Application.Quit(); Debug.Log("Quit"); }
    public void ExitGame() => SceneManager.LoadScene("START_MENU");
}