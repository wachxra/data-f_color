using UnityEngine;
using System.Collections;

public class Box : TileObject
{
    public ColorType colorType;
    private Goal currentGoal;

    public SpriteRenderer spriteRenderer;

    public LevelManager levelManager;

    [HideInInspector] public bool isBlocked = false;

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

    public bool TryMoveBox(Vector2 direction, bool isMainPush = false)
    {
        if (isBlocked) return false;

        Vector2 target = (Vector2)transform.position + direction;

        Collider2D[] hits = Physics2D.OverlapPointAll(target);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Wall"))
                return false;

            if (hit.CompareTag("Box"))
            {
                Box other = hit.GetComponent<Box>();
                if (other != null)
                {
                    if (other.isBlocked) return false;

                    if (isMainPush)
                    {
                        bool explode;
                        GameObject exp;
                        GameObject result = levelManager.mergeRule.GetResult(colorType, other.colorType, out explode, out exp);

                        if (result != null || explode)
                        {
                            MergeAndSpawn(other, target);
                            return true;
                        }
                    }

                    bool moved = other.TryMoveBox(direction, false);
                    if (!moved) return false;

                    if (isMainPush)
                        MergeAndSpawn(other, target);

                    return true;
                }
            }
        }

        transform.position = target;
        gridPos += Vector2Int.RoundToInt(direction);
        CheckGoalStatus();
        return true;
    }

    void MergeAndSpawn(Box other, Vector2 spawnPos)
    {
        bool explode;
        GameObject explosionPrefab;
        GameObject resultPrefab = levelManager.mergeRule.GetResult(colorType, other.colorType, out explode, out explosionPrefab);

        if (explode)
        {
            isBlocked = true;

            Destroy(gameObject);
            Destroy(other.gameObject);

            if (explosionPrefab != null)
            {
                GameObject explodeObj = Instantiate(explosionPrefab, spawnPos, Quaternion.identity);
                Box explodeBox = explodeObj.GetComponent<Box>();
                if (explodeBox != null)
                {
                    explodeBox.levelManager = levelManager;
                    explodeBox.gridPos = Vector2Int.RoundToInt(spawnPos);
                    explodeBox.isBlocked = true;
                    explodeBox.StartCoroutine(explodeBox.BlinkThenExplode(3f));
                }
            }

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

    public IEnumerator BlinkThenExplode(float duration)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(0.25f);
            elapsed += 0.25f;
        }

        ExplodeArea();

        Destroy(gameObject);
    }

    private void ExplodeArea()
    {
        Vector2 center = transform.position;
        float radius = 1.1f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Box") && hit.gameObject != this.gameObject)
            {
                Box otherBox = hit.GetComponent<Box>();
                if (otherBox != null)
                {
                    if (!otherBox.isBlocked)
                    {
                        Destroy(otherBox.gameObject);
                    }
                }
            }

            if (hit.CompareTag("Wall"))
            {
                Destroy(hit.gameObject);
            }

            if (hit.CompareTag("Player"))
            {
                if (levelManager != null && levelManager.gameManager != null)
                {
                    levelManager.gameManager.TakeDamage(1);
                }
            }
        }

        if (levelManager != null && levelManager.traps != null)
        {
            foreach (var trap in levelManager.traps)
            {
                if (!trap.triggered && Vector2.Distance(trap.position, center) <= radius)
                {
                    trap.triggered = true;

                    if (trap.prefab != null)
                    {
                        GameObject trapObj = Instantiate(trap.prefab, trap.position, Quaternion.identity);
                        Box trapBox = trapObj.GetComponent<Box>();
                        if (trapBox != null)
                        {
                            trapBox.levelManager = levelManager;
                            trapBox.gridPos = Vector2Int.RoundToInt(trap.position);
                            trapBox.StartCoroutine(trapBox.BlinkThenExplode(3f));
                        }
                    }
                }
            }
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