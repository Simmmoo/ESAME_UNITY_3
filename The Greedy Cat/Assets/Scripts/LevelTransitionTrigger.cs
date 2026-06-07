using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitionTrigger : MonoBehaviour
{
    private ChangeScene changeSceneScript;
    private bool hasTriggered = false;
    public string nextSceneName;
    public string currentSceneName;

    private void Awake()
    {
        //changeSceneScript = FindFirstObjectByType<ChangeScene>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Passaggio livello avviato...");

            SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(currentSceneName);

        }
    }
}