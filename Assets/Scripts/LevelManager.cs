using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject wallPrefab;
    public GameObject goalPrefab;
    public GameObject trapPrefab;

    [Header("Player Settings")]
    public PlayerSettings playerSettings;

    [Header("Wall Settings")]
    public WallSettings wallSettings;

    [Header("Goal Settings")]
    public GoalSettings goalSettings;

    [Header("Trap Settings")]
    public TrapSettings trapSettings;

    [Header("Box Settings")]
    public BoxSettings boxSettings;

    [Header("Random Settings")]
    public RandomSettings randomSettings;

    [Header("Merge Rules")]
    public BoxMergeRule mergeRule;

    [HideInInspector] public List<Box> boxes = new List<Box>();
    [HideInInspector] public List<Goal> goals = new List<Goal>();
    [HideInInspector] public List<TrapData> traps = new List<TrapData>();

    public GameManager gameManager;

    void Start() { }

    public void LoadLevelData()
    {
        boxes.Clear();
        goals.Clear();
        traps.Clear();
    }

    public void SpawnObjects()
    {
        if (playerSettings.useFixed) SpawnPlayer();
        if (wallSettings.useFixed) SpawnWalls();

        if (goalSettings.useFixed && goalSettings.fixedCount > 0) SpawnFixedGoals();
        if (goalSettings.useRandom && goalSettings.randomCount > 0) SpawnRandomGoals();

        if (boxSettings.useFixed) SpawnFixedBoxes();
        if (boxSettings.useRandom) SpawnRandomBoxes();

        if (trapSettings.useFixed && trapSettings.fixedCount > 0) SpawnFixedTraps();
        if (trapSettings.useRandom && trapSettings.randomCount > 0) SpawnRandomTraps();
    }

    #region Spawn Methods
    void SpawnPlayer()
    {
        Instantiate(playerPrefab, playerSettings.spawnPoint, Quaternion.identity);
    }

    void SpawnWalls()
    {
        if (wallPrefab == null) return;
        foreach (var pos in wallSettings.wallPositions)
            Instantiate(wallPrefab, pos, Quaternion.identity);
    }

    void SpawnFixedGoals()
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var point in goalSettings.fixedPoints)
            if (!occupied.Contains(point)) availablePoints.Add(point);

        ShuffleList(availablePoints);
        int spawnCount = Mathf.Min(goalSettings.fixedCount, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = availablePoints[i];
            GameObject g = Instantiate(goalPrefab, pos, Quaternion.identity);
            goals.Add(g.GetComponent<Goal>());
        }
    }

    void SpawnRandomGoals()
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        for (int i = 0; i < goalSettings.randomCount; i++)
        {
            Vector2 pos = GetRandomIntPosition(occupied);
            GameObject g = Instantiate(goalPrefab, pos, Quaternion.identity);
            goals.Add(g.GetComponent<Goal>());
            occupied.Add(pos);
        }
    }

    void SpawnFixedTraps()
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var point in trapSettings.fixedPoints)
            if (!occupied.Contains(point)) availablePoints.Add(point);

        ShuffleList(availablePoints);
        int spawnCount = Mathf.Min(trapSettings.fixedCount, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = availablePoints[i];
            traps.Add(new TrapData(pos, trapPrefab));
        }
    }

    void SpawnRandomTraps()
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        for (int i = 0; i < trapSettings.randomCount; i++)
        {
            Vector2 pos = GetRandomIntPosition(occupied);
            traps.Add(new TrapData(pos, trapPrefab));
            occupied.Add(pos);
        }
    }

    void SpawnFixedBoxes()
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var group in boxSettings.spawnPointGroups)
            foreach (var p in group.points)
                if (!occupied.Contains(p)) availablePoints.Add(p);

        ShuffleList(availablePoints);

        foreach (var req in boxSettings.colorSpawnRequests)
        {
            for (int i = 0; i < req.count; i++)
            {
                if (availablePoints.Count == 0) return;
                Vector2 pos = availablePoints[0]; availablePoints.RemoveAt(0);
                GameObject prefab = GetRandomPrefabOfColor(req.color);
                if (prefab == null) continue;

                GameObject b = Instantiate(prefab, pos, Quaternion.identity);
                Box box = b.GetComponent<Box>();
                box.colorType = req.color;
                box.levelManager = this;
                boxes.Add(box);
            }
        }
    }

    void SpawnRandomBoxes()
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        foreach (var boxInfo in boxSettings.randomBoxes)
        {
            for (int i = 0; i < boxInfo.count; i++)
            {
                Vector2 pos = GetRandomIntPosition(occupied);
                GameObject prefab = GetRandomPrefabOfColor(boxInfo.color);
                if (prefab == null) continue;

                GameObject b = Instantiate(prefab, pos, Quaternion.identity);
                Box box = b.GetComponent<Box>();
                box.colorType = boxInfo.color;
                box.levelManager = this;
                boxes.Add(box);
                occupied.Add(pos);
            }
        }
    }
    #endregion

    #region Utility
    HashSet<Vector2> GetOccupiedPositions()
    {
        HashSet<Vector2> occupied = new HashSet<Vector2>();
        foreach (var g in goals) occupied.Add(g.transform.position);
        foreach (var b in boxes) occupied.Add(b.transform.position);
        occupied.Add(playerSettings.spawnPoint);
        foreach (var w in wallSettings.wallPositions) occupied.Add(w);
        return occupied;
    }

    Vector2 GetRandomIntPosition(HashSet<Vector2> occupied)
    {
        Vector2 pos = Vector2.zero;
        int tries = 0;
        do
        {
            pos = new Vector2(
                Random.Range(randomSettings.randomMin.x, randomSettings.randomMax.x + 1),
                Random.Range(randomSettings.randomMin.y, randomSettings.randomMax.y + 1)
            );
            tries++;
        } while (occupied.Contains(pos) && tries < 50);
        return pos;
    }

    GameObject GetRandomPrefabOfColor(ColorType color)
    {
        List<GameObject> matched = new List<GameObject>();
        foreach (var p in boxSettings.boxColorPrefabs)
            if (p.color == color) matched.Add(p.prefab);
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
    #endregion
}

#region Settings Classes
[System.Serializable]
public class PlayerSettings { public Vector2 spawnPoint = Vector2.zero; public bool useFixed = true; }
[System.Serializable]
public class WallSettings { public List<Vector2> wallPositions; public bool useFixed = true; }
[System.Serializable]
public class GoalSettings
{
    public bool useFixed = true;
    public bool useRandom = false;
    public List<Vector2> fixedPoints;
    public int fixedCount = 1;
    public int randomCount = 3;
}
[System.Serializable]
public class TrapSettings
{
    public bool useFixed = true;
    public bool useRandom = false;
    public List<Vector2> fixedPoints;
    public int fixedCount = 1;
    public int randomCount = 2;
}
[System.Serializable]
public class BoxSettings
{
    public List<BoxColorPrefabPair> boxColorPrefabs;
    public List<SpawnPointGroup> spawnPointGroups;
    public List<ColorSpawnRequest> colorSpawnRequests;
    public List<RandomBoxInfo> randomBoxes;
    public bool useFixed = true;
    public bool useRandom = false;
}
[System.Serializable]
public class RandomSettings
{
    public Vector2Int randomMin = new Vector2Int(-5, -5);
    public Vector2Int randomMax = new Vector2Int(5, 5);
}
#endregion

#region Data Classes
[System.Serializable]
public class TrapData { public Vector2 position; public bool triggered = false; public GameObject prefab; public TrapData(Vector2 pos, GameObject p) { position = pos; prefab = p; triggered = false; } }
[System.Serializable]
public class BoxColorPrefabPair { public GameObject prefab; public ColorType color; }
[System.Serializable]
public class SpawnPointGroup { public string groupName; public List<Vector2> points; }
[System.Serializable]
public class ColorSpawnRequest { public ColorType color; public int count; }
[System.Serializable]
public class RandomBoxInfo { public ColorType color; public int count; }
#endregion