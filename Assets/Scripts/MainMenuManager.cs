using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Tutorial Settings")]
    public GameObject tutorialPanel;
    public string gameSceneName;
    private bool tutorialOpened = false;

    public void StartGame(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            tutorialOpened = true;
            gameSceneName = sceneName;
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PlayAgain()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void Update()
    {
        if (tutorialOpened && Input.GetKeyDown(KeyCode.Space))
        {
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
    }
}