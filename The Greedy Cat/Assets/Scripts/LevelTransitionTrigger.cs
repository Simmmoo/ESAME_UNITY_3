using UnityEngine;

public class LevelTransitionTrigger : MonoBehaviour
{
    private ChangeScene changeSceneScript;
    private bool hasTriggered = false;

    private void Awake()
    {
        changeSceneScript = FindFirstObjectByType<ChangeScene>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            if (changeSceneScript != null)
            {
                Debug.Log("Passaggio livello avviato...");
                changeSceneScript.StartLevelTransition();
            }
            else
            {
                Debug.LogError("Manca lo script ChangeScene nella scena!");
            }
        }
    }
}