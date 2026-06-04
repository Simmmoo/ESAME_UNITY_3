using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionTrigger : MonoBehaviour
{
    private bool hasTriggered = false;

    public string nextSceneName;
    public string currentSceneName;

    // Numero del livello che viene SBLOCCATO toccando questo trigger.
    // Es: nel LVL_01 metti 2, nel LVL_02 metti 3.
    [Header("Progressione")]
    public int livelloDaSbloccare = 2;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Passaggio livello avviato...");

            // Salva il livello sbloccato solo se è più alto di quello già salvato
            int highestUnlocked = PlayerPrefs.GetInt("HighestLevelUnlocked", 1);
            if (livelloDaSbloccare > highestUnlocked)
            {
                PlayerPrefs.SetInt("HighestLevelUnlocked", livelloDaSbloccare);
                PlayerPrefs.Save();
                Debug.Log("Livello sbloccato: " + livelloDaSbloccare);
            }

            SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(currentSceneName);
        }
    }
}