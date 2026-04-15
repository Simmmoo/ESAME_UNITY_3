using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{  public void LoadGame()
    {
        SceneManager.LoadScene("Main_Scene");
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Start_Scene");
    }
}
