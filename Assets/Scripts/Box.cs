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

        Collider2D hit = Physics2D.OverlapPoint(target);
        if (hit != null && hit.CompareTag("Box"))
        {
            Box other = hit.GetComponent<Box>();
            TryMerge(other);
            return;
        }

        transform.position = target;
        CheckGoalStatus();
    }

    void TryMerge(Box other)
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
            Vector3 spawnPos = (transform.position + other.transform.position) / 2f;
            GameObject merged = Instantiate(resultPrefab, spawnPos, Quaternion.identity);

            Destroy(gameObject);
            Destroy(other.gameObject);
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
