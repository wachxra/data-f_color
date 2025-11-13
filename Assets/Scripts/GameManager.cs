using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Level References")]
    public LevelManager levelManager;

    [Header("UI Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Goal Tracking")]
    public int goalsCompleted;
    public int totalGoals;

    [Header("Player HP")]
    public int playerHP = 3;

    void Start()
    {
        StartLevel();

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void StartLevel()
    {
        goalsCompleted = 0;
        levelManager.LoadLevelData();
        levelManager.SpawnObjects();
        totalGoals = levelManager.goals.Count;
    }

    public void CheckWinCondition()
    {
        goalsCompleted = 0;
        foreach (Goal g in levelManager.goals)
        {
            if (g.isCompleted)
                goalsCompleted++;
        }

        if (goalsCompleted >= totalGoals)
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        Debug.Log("🎉 You Win! All goals completed!");
        Time.timeScale = 0f;
        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0f;
        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void TakeDamage(int damage)
    {
        playerHP -= damage;
        if (playerHP <= 0)
        {
            GameOver();
        }
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}