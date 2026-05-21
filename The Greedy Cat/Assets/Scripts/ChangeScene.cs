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
        SceneManager.LoadScene("LVL_01", LoadSceneMode.Additive);
        startScreenCanvasGrp.alpha = 0;
        fadePanel.color = new Color(0, 0, 0, 0);
        fadePanel.gameObject.SetActive(false);
        startScreenCanvasGrp.blocksRaycasts = false;
    }
    public void LoadSpecificLevel(string levelName) => SceneManager.LoadScene(levelName);
    public void QuitGame() { Application.Quit(); Debug.Log("Quit"); }
    public void ExitGame() => SceneManager.LoadScene("START_MENU");
}