using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Vector2 playerSpawnPoint = Vector2.zero;
    public bool playerUseFixed = true;

    [Header("Wall")]
    public GameObject wallPrefab;
    public List<Vector2> wallPositions;
    public bool wallUseFixed = true;

    [Header("Goals")]
    public List<GoalSpawnInfo> goalSpawnInfos;
    public bool goalUseFixed = true;
    public bool goalUseRandom = false;

    [Header("Boxes")]
    public List<BoxColorPrefabPair> boxColorPrefabs;
    public List<SpawnPointGroup> spawnPointGroups;
    public List<ColorSpawnRequest> colorSpawnRequests;
    public List<RandomBoxInfo> randomBoxes = new List<RandomBoxInfo>();
    public bool boxUseFixed = true;
    public bool boxUseRandom = false;

    [Header("Bomb Traps")]
    public List<FixedTrapInfo> fixedTraps = new List<FixedTrapInfo>();
    public List<RandomTrapInfo> randomTraps = new List<RandomTrapInfo>();
    public bool trapUseFixed = false;
    public bool trapUseRandom = false;

    [Header("Random All Settings")]
    public Vector2Int randomMin = new Vector2Int(-5, -5);
    public Vector2Int randomMax = new Vector2Int(5, 5);
    public int randomGoalCount = 3;

    [Header("Merge Rules")]
    public BoxMergeRule mergeRule;

    public List<Box> boxes = new List<Box>();
    public List<Goal> goals = new List<Goal>();
    public List<TrapData> traps = new List<TrapData>();

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
        if (playerUseFixed) SpawnPlayer();
        if (wallUseFixed) SpawnWalls();

        if (goalUseFixed) SpawnFixedGoals();
        if (goalUseRandom) SpawnRandomGoals();

        if (boxUseFixed) SpawnFixedBoxes();
        if (boxUseRandom) SpawnRandomBoxes();

        if (trapUseFixed) SpawnFixedTraps();
        if (trapUseRandom) SpawnRandomTraps();
    }

    void SpawnPlayer()
    {
        Instantiate(playerPrefab, playerSpawnPoint, Quaternion.identity);
    }

    void SpawnWalls()
    {
        if (wallPrefab == null) return;
        foreach (var pos in wallPositions)
        {
            Instantiate(wallPrefab, pos, Quaternion.identity);
        }
    }

    void SpawnFixedGoals()
    {
        foreach (var info in goalSpawnInfos)
        {
            for (int i = 0; i < info.count; i++)
            {
                Vector2 pos = info.startPos + new Vector2(i * info.spacing.x, i * info.spacing.y);
                GameObject g = Instantiate(info.prefab, pos, Quaternion.identity);
                goals.Add(g.GetComponent<Goal>());
            }
        }
    }

    void SpawnRandomGoals()
    {
        HashSet<Vector2> occupied = new HashSet<Vector2>();
        foreach (var g in goals) occupied.Add(g.transform.position);
        occupied.Add(playerSpawnPoint);
        foreach (var w in wallPositions) occupied.Add(w);

        for (int i = 0; i < randomGoalCount; i++)
        {
            Vector2 pos = GetRandomIntPosition(occupied);
            var info = goalSpawnInfos[Random.Range(0, goalSpawnInfos.Count)];
            GameObject g = Instantiate(info.prefab, pos, Quaternion.identity);
            goals.Add(g.GetComponent<Goal>());
            occupied.Add(pos);
        }
    }

    void SpawnFixedBoxes()
    {
        HashSet<Vector2> occupied = new HashSet<Vector2>();
        foreach (var g in goals) occupied.Add(g.transform.position);

        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var group in spawnPointGroups)
            foreach (var p in group.points)
                if (!occupied.Contains(p))
                    availablePoints.Add(p);

        ShuffleList(availablePoints);

        foreach (var req in colorSpawnRequests)
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
        HashSet<Vector2> occupied = new HashSet<Vector2>();
        foreach (var g in goals) occupied.Add(g.transform.position);
        foreach (var b in boxes) occupied.Add(b.transform.position);
        occupied.Add(playerSpawnPoint);
        foreach (var w in wallPositions) occupied.Add(w);

        foreach (var boxInfo in randomBoxes)
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

    void SpawnFixedTraps()
    {
        if (fixedTraps == null || fixedTraps.Count == 0) return;

        HashSet<Vector2> occupied = new HashSet<Vector2>();
        foreach (var g in goals) occupied.Add(g.transform.position);
        foreach (var b in boxes) occupied.Add(b.transform.position);
        occupied.Add(playerSpawnPoint);
        foreach (var w in wallPositions) occupied.Add(w);

        foreach (var trapInfo in fixedTraps)
        {
            if (occupied.Contains(trapInfo.position)) continue;

            traps.Add(new TrapData(trapInfo.position, trapInfo.prefab));
            occupied.Add(trapInfo.position);
        }
    }

    void SpawnRandomTraps()
    {
        HashSet<Vector2> occupied = new HashSet<Vector2>();
        foreach (var g in goals) occupied.Add(g.transform.position);
        foreach (var b in boxes) occupied.Add(b.transform.position);
        occupied.Add(playerSpawnPoint);
        foreach (var w in wallPositions) occupied.Add(w);

        foreach (var trapInfo in randomTraps)
        {
            for (int i = 0; i < trapInfo.count; i++)
            {
                Vector2 pos = GetRandomIntPosition(occupied);
                traps.Add(new TrapData(pos, trapInfo.prefab));
                occupied.Add(pos);
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
            if (p.color == color)
                matched.Add(p.prefab);

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
public class TrapData
{
    public Vector2 position;
    public bool triggered = false;
    public GameObject prefab;

    public TrapData(Vector2 pos, GameObject p)
    {
        position = pos;
        prefab = p;
        triggered = false;
    }
}

[System.Serializable]
public class FixedTrapInfo
{
    public Vector2 position;
    public GameObject prefab;
}

[System.Serializable]
public class RandomTrapInfo
{
    public GameObject prefab;
    public int count;
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