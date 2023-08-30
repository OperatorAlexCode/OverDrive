using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public string GameSceneName = "InGame";
    public string MainMenuSceneName = "MainMenu";

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
