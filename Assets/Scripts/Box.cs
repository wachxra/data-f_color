using UnityEngine;

public class Box : TileObject
{
    public ColorType colorType;
    private Goal currentGoal;

    public LevelManager levelManager;

    private void Awake()
    {
        if (levelManager == null)
        {
            levelManager = Object.FindFirstObjectByType<LevelManager>();
        }
    }

    public void Initialize(Vector2Int pos)
    {
        gridPos = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    public void Move(Vector2 direction)
    {
        Vector2 target = (Vector2)transform.position + direction;

        Collider2D[] hits = Physics2D.OverlapBoxAll(target, Vector2.one * 0.9f, 0f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Box") && hit.gameObject != this.gameObject)
            {
                Box other = hit.GetComponent<Box>();
                MergeAndSpawn(other, target);
                return;
            }
        }

        transform.position = target;
        gridPos += Vector2Int.RoundToInt(direction);
        CheckGoalStatus();
    }

    void MergeAndSpawn(Box other, Vector2 spawnPos)
    {
        bool explode;
        GameObject resultPrefab = levelManager.GetMergeResult(colorType, other.colorType, out explode);

        if (explode)
        {
            Destroy(gameObject);
            Destroy(other.gameObject);
            levelManager.gameManager.TakeDamage(1);
            return;
        }

        if (resultPrefab != null)
        {
            GameObject merged = Instantiate(resultPrefab, spawnPos, Quaternion.identity);

            Box mergedBox = merged.GetComponent<Box>();
            if (mergedBox != null)
            {
                mergedBox.levelManager = levelManager;
                mergedBox.gridPos = Vector2Int.RoundToInt(spawnPos);
            }

            Destroy(gameObject);
            Destroy(other.gameObject);
        }
        else
        {
            transform.position = spawnPos;
            gridPos = Vector2Int.RoundToInt(spawnPos);
        }
    }

    private void CheckGoalStatus()
    {
        Goal newGoal = null;
        Goal[] allGoals = FindObjectsByType<Goal>(FindObjectsSortMode.None);

        foreach (Goal g in allGoals)
        {
            if (Vector2.Distance(g.transform.position, transform.position) < 0.1f)
            {
                newGoal = g;
                break;
            }
        }

        if (currentGoal != newGoal)
        {
            if (currentGoal != null)
                currentGoal.OnBoxRemoved();

            if (newGoal != null)
                newGoal.OnBoxPlaced(this);

            currentGoal = newGoal;
        }
    }
}