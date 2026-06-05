using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour
{
    [Header("Impostazioni Fade (Dissolvenza)")]
    public Image fadeCanvasImage;
    public float fadeDuration = 1f;
    public CanvasGroup startScreenCanvasGrp;
    public Image fadePanel;


    private void Start()
    {
        if (fadeCanvasImage != null)
        {
            fadeCanvasImage.gameObject.SetActive(true);
            fadeCanvasImage.color = new Color(0, 0, 0, 0);
        }
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

    public void LoadGame()
    {
        StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator LoadGameCoroutine()
    {
        // Scarica tutti i livelli additivi e aspetta che siano effettivamente scaricati
        List<AsyncOperation> unloadOps = new List<AsyncOperation>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scena = SceneManager.GetSceneAt(i);
            if (scena.name != "START_MENU")
                unloadOps.Add(SceneManager.UnloadSceneAsync(scena.buildIndex));
        }

        // Aspetta che tutti gli scaricamenti siano completati
        foreach (AsyncOperation op in unloadOps)
        {
            if (op != null)
                yield return new WaitUntil(() => op.isDone);
        }

        startScreenCanvasGrp.alpha = 0;
        startScreenCanvasGrp.blocksRaycasts = false;
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.gameObject.SetActive(false);

        // Solo ora carica LVL_01, quando la memoria è pulita
        if (GameManager.Instance != null)
            GameManager.Instance.SelezionaLivello1();
    }
    public void LoadSpecificLevel(string levelName) => SceneManager.LoadScene(levelName);
    public void QuitGame() { Application.Quit(); Debug.Log("Quit"); }
    public void ExitGame() => SceneManager.LoadScene("START_MENU");
}