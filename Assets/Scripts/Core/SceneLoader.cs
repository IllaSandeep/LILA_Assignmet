using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadGame()
    {
        LoadGameScene();
    }

    public void LoadMainMenu()
    {
        LoadMainMenuScene();
    }

    public static void LoadGameScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "Game"
        );
    }

    public static void LoadMainMenuScene()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            "MainMenu"
        );
    }

    public void QuitGame()
    {
        Debug.Log(
            "QUIT GAME"
        );

        Application.Quit();
    }
}
