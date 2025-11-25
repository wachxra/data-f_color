using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Preset: Prefab–Color Pairs")]
    public List<BoxColorPrefabPair> boxColorPrefabs;

    [Header("Spawn Points Groups")]
    public List<SpawnPointGroup> spawnPointGroups;

    [Header("Spawn Request (Color → Count)")]
    public List<ColorSpawnRequest> colorSpawnRequests;

    [Header("Goals")]
    public List<GoalSpawnInfo> goalSpawnInfos;

    [Header("Merge Rules")]
    public BoxMergeRule mergeRule;

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
        Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        foreach (var info in goalSpawnInfos)
        {
            for (int i = 0; i < info.count; i++)
            {
                Vector2 pos = info.startPos + new Vector2(i * info.spacing.x, i * info.spacing.y);
                GameObject g = Instantiate(info.prefab, pos, Quaternion.identity);
                goals.Add(g.GetComponent<Goal>());
            }
        }

        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var group in spawnPointGroups)
        {
            foreach (var p in group.points)
                availablePoints.Add(p);
        }

        ShuffleList(availablePoints);

        foreach (var req in colorSpawnRequests)
        {
            for (int i = 0; i < req.count; i++)
            {
                if (availablePoints.Count == 0)
                {
                    return;
                }

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
            }
        }
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

    public GameObject GetMergeResult(ColorType a, ColorType b, out bool explode)
    {
        return mergeRule.GetResult(a, b, out explode);
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
