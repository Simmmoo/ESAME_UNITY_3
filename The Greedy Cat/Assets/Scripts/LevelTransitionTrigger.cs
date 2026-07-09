using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    private bool hasTriggered = false;
    public string nextSceneName;
    public string currentSceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            Debug.Log("Passaggio livello avviato...");

            ChangeScene.Instance.FadeToScene(currentSceneName, nextSceneName);
        }
    }
}