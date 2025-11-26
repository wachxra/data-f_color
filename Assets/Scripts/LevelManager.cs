using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Vector2 playerSpawnPoint = Vector2.zero;

    [Header("Preset: Prefabs Color Pairs")]
    public List<BoxColorPrefabPair> boxColorPrefabs;

    [Header("Box: Spawn Points Groups")]
    public List<SpawnPointGroup> spawnPointGroups;

    [Header("Box: Spawn Request")]
    public List<ColorSpawnRequest> colorSpawnRequests;

    [Header("Goals")]
    public List<GoalSpawnInfo> goalSpawnInfos;

    [Header("Wall")]
    public GameObject wallPrefab;
    public List<Vector2> wallPositions;

    [Header("Merge Rules")]
    public BoxMergeRule mergeRule;

    [Header("Random All Settings")]
    public bool randomAll = false;
    public Vector2Int randomMin = new Vector2Int(-5, -5);
    public Vector2Int randomMax = new Vector2Int(5, 5);

    [Header("Random Count Settings")]
    public int randomGoalCount = 3;
    public List<RandomBoxInfo> randomBoxes = new List<RandomBoxInfo>();

    public List<Box> boxes = new List<Box>();
    public List<Goal> goals = new List<Goal>();

    public GameManager gameManager;

    void Start() { }

    public void LoadLevelData()
    {
        boxes.Clear();
        goals.Clear();
    }

    public void SpawnObjects()
    {
        HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();

        Vector2 playerPos = playerSpawnPoint;
        int attempt = 0;
        while (occupiedPositions.Contains(playerPos) && attempt < 10)
        {
            playerPos += Vector2.right;
            attempt++;
        }
        Instantiate(playerPrefab, playerPos, Quaternion.identity);
        occupiedPositions.Add(playerPos);

        if (wallPrefab != null)
        {
            foreach (var pos in wallPositions)
            {
                Vector2 spawnPos = pos;
                attempt = 0;
                while (occupiedPositions.Contains(spawnPos) && attempt < 10)
                {
                    spawnPos += Vector2.right;
                    attempt++;
                }
                Instantiate(wallPrefab, spawnPos, Quaternion.identity);
                occupiedPositions.Add(spawnPos);
            }
        }

        foreach (var info in goalSpawnInfos)
        {
            for (int i = 0; i < info.count; i++)
            {
                Vector2 pos = info.startPos + new Vector2(i * info.spacing.x, i * info.spacing.y);
                attempt = 0;
                while (occupiedPositions.Contains(pos) && attempt < 10)
                {
                    pos += Vector2.right;
                    attempt++;
                }
                GameObject g = Instantiate(info.prefab, pos, Quaternion.identity);
                goals.Add(g.GetComponent<Goal>());
                occupiedPositions.Add(pos);
            }
        }

        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var group in spawnPointGroups)
            foreach (var p in group.points)
                if (!occupiedPositions.Contains(p))
                    availablePoints.Add(p);

        ShuffleList(availablePoints);

        foreach (var req in colorSpawnRequests)
        {
            for (int i = 0; i < req.count; i++)
            {
                if (availablePoints.Count == 0) return;

                Vector2 pos = availablePoints[0];
                availablePoints.RemoveAt(0);

                GameObject prefab = GetRandomPrefabOfColor(req.color);
                if (prefab == null)
                {
                    Debug.LogError("No prefab assigned for color: " + req.color);
                    continue;
                }

                GameObject b = Instantiate(prefab, pos, Quaternion.identity);
                Box box = b.GetComponent<Box>();
                box.colorType = req.color;
                box.levelManager = this;

                boxes.Add(box);
                occupiedPositions.Add(pos);
            }
        }

        if (randomAll)
        {
            for (int i = 0; i < randomGoalCount; i++)
            {
                Vector2 pos = GetRandomIntPosition(occupiedPositions);
                if (occupiedPositions.Contains(pos)) continue;

                var info = goalSpawnInfos[Random.Range(0, goalSpawnInfos.Count)];
                GameObject g = Instantiate(info.prefab, pos, Quaternion.identity);
                goals.Add(g.GetComponent<Goal>());
                occupiedPositions.Add(pos);
            }

            foreach (var boxInfo in randomBoxes)
            {
                for (int i = 0; i < boxInfo.count; i++)
                {
                    Vector2 pos = GetRandomIntPosition(occupiedPositions);
                    if (occupiedPositions.Contains(pos)) continue;

                    GameObject prefab = GetRandomPrefabOfColor(boxInfo.color);
                    if (prefab == null)
                    {
                        Debug.LogError("No prefab assigned for color: " + boxInfo.color);
                        continue;
                    }

                    GameObject b = Instantiate(prefab, pos, Quaternion.identity);
                    Box box = b.GetComponent<Box>();
                    box.colorType = boxInfo.color;
                    box.levelManager = this;
                    boxes.Add(box);
                    occupiedPositions.Add(pos);
                }
            }
        }
    }

    Vector2 GetRandomIntPosition(HashSet<Vector2> occupied)
    {
        Vector2 pos = Vector2.zero;
        int tries = 0;
        do
        {
            pos = new Vector2(
                Random.Range(randomMin.x, randomMax.x + 1),
                Random.Range(randomMin.y, randomMax.y + 1)
            );
            tries++;
        } while (occupied.Contains(pos) && tries < 50);
        return pos;
    }

    GameObject GetRandomPrefabOfColor(ColorType color)
    {
        List<GameObject> matched = new List<GameObject>();
        foreach (var p in boxColorPrefabs)
        {
            if (p.color == color)
                matched.Add(p.prefab);
        }
        if (matched.Count == 0) return null;
        return matched[Random.Range(0, matched.Count)];
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    public GameObject GetMergeResult(ColorType a, ColorType b, out bool explode, out GameObject explosionPrefab)
    {
        return mergeRule.GetResult(a, b, out explode, out explosionPrefab);
    }
}

[System.Serializable]
public class BoxColorPrefabPair
{
    public GameObject prefab;
    public ColorType color;
}

[System.Serializable]
public class SpawnPointGroup
{
    public string groupName;
    public List<Vector2> points;
}

[System.Serializable]
public class ColorSpawnRequest
{
    public ColorType color;
    public int count;
}

[System.Serializable]
public class GoalSpawnInfo
{
    public GameObject prefab;
    public Vector2 startPos;
    public int count = 1;
    public Vector2 spacing = new Vector2(1, 0);
}

[System.Serializable]
public class RandomBoxInfo
{
    public ColorType color;
    public int count;
}
