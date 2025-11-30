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
    public int totalGoalsCurrentGroup;

    [Header("Player HP")]
    public int playerHP = 3;
    public List<Image> heartImages;

    public int currentGroupIndex = 0;

    void Start()
    {
        StartLevel();

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void StartLevel()
    {
        currentGroupIndex = 0;
        goalsCompleted = 0;

        if (levelManager != null)
        {
            levelManager.LoadLevelData();
            levelManager.SpawnObjects();
            UpdateGroupGoalsTarget();
        }

        ResetHeartsUI();
    }

    void UpdateGroupGoalsTarget()
    {
        if (levelManager != null)
        {
            totalGoalsCurrentGroup = levelManager.goals.Count;
        }
    }

    public bool IsPositionInsideBounds(Vector2 targetPos)
    {
        if (levelManager == null || levelManager.levelGroups == null) return true;
        if (currentGroupIndex >= levelManager.levelGroups.Count) return false;

        var currentGroup = levelManager.levelGroups[currentGroupIndex];

        int x = Mathf.RoundToInt(targetPos.x);
        int y = Mathf.RoundToInt(targetPos.y);

        bool insideOuter = x >= currentGroup.boundMin.x && x <= currentGroup.boundMax.x;
        bool insideY = y >= currentGroup.boundMin.y && y <= currentGroup.boundMax.y;

        if (!insideOuter || !insideY) return false;

        if (currentGroup.innerBounds != null)
        {
            foreach (var inner in currentGroup.innerBounds)
            {
                bool insideInnerX = x >= inner.min.x && x <= inner.max.x;
                bool insideInnerY = y >= inner.min.y && y <= inner.max.y;

                if (insideInnerX && insideInnerY)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void CheckWinCondition()
    {
        if (levelManager == null) return;

        int completedInThisGroup = 0;
        foreach (Goal g in levelManager.goals)
        {
            if (g != null && g.isCompleted)
                completedInThisGroup++;
        }

        goalsCompleted = completedInThisGroup;

        if (goalsCompleted >= totalGoalsCurrentGroup && totalGoalsCurrentGroup > 0)
        {
            OpenDoorsInCurrentGroup();
        }
    }

    void OpenDoorsInCurrentGroup()
    {
        if (levelManager != null)
        {
            foreach (var door in levelManager.doors)
            {
                if (door != null) door.OpenDoor();
            }
        }
    }

    public void AdvanceLevel()
    {
        int nextIndex = currentGroupIndex + 1;

        if (levelManager != null && nextIndex < levelManager.levelGroups.Count)
        {
            var nextGroup = levelManager.levelGroups[nextIndex];

            GameObject player = levelManager.playerInstance;
            if (player != null)
            {
                player.transform.position = nextGroup.playerSettings.spawnPoint;
            }

            levelManager.ClearCurrentLevel();

            currentGroupIndex = nextIndex;
            levelManager.SpawnLevel(currentGroupIndex);

            UpdateGroupGoalsTarget();
            Debug.Log("Group: " + currentGroupIndex);
        }
        else
        {
            WinGame();
        }
    }

    public void WinGame()
    {
        Debug.Log("You Win!");
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

        if (heartImages != null)
        {
            for (int i = 0; i < damage; i++)
            {
                if (heartImages.Count > 0)
                {
                    Image heart = heartImages[heartImages.Count - 1];
                    if (heart != null) heart.gameObject.SetActive(false);
                    heartImages.RemoveAt(heartImages.Count - 1);
                }
            }
        }

        if (playerHP <= 0)
        {
            GameOver();
        }
    }

    private void ResetHeartsUI()
    {
        if (heartImages == null) return;

        foreach (var heart in heartImages)
        {
            if (heart != null) heart.gameObject.SetActive(true);
        }
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}