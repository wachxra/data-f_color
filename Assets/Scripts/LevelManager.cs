using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject wallPrefab;
    public GameObject goalPrefab;
    public GameObject doorPrefab;
    public GameObject trapPrefab;
    public List<BoxColorPrefabPair> boxColorPrefabs;

    [Header("References")]
    public BoxMergeRule mergeRule;
    public GameManager gameManager;

    [HideInInspector] public GameObject playerInstance;

    [HideInInspector] public List<Box> boxes = new List<Box>();
    [HideInInspector] public List<Goal> goals = new List<Goal>();
    [HideInInspector] public List<Door> doors = new List<Door>();
    [HideInInspector] public List<TrapData> traps = new List<TrapData>();

    [HideInInspector] public List<GameObject> spawnedWalls = new List<GameObject>();

    [Header("Level Groups")]
    public List<LevelGroup> levelGroups;

    void Awake()
    {
        boxes = new List<Box>();
        goals = new List<Goal>();
        doors = new List<Door>();
        traps = new List<TrapData>();
        spawnedWalls = new List<GameObject>();
    }

    void Start() { }

    void OnDrawGizmosSelected()
    {
        if (levelGroups == null) return;

        foreach (var group in levelGroups)
        {
            Gizmos.color = Color.cyan;
            DrawBoundGizmo(group.boundMin, group.boundMax);

            if (group.randomSettings != null)
            {
                Gizmos.color = Color.red;
                DrawBoundGizmo(group.randomSettings.randomMin, group.randomSettings.randomMax);
            }

            Gizmos.color = new Color(1f, 0.5f, 0f);
            if (group.innerBounds != null)
            {
                foreach (var bound in group.innerBounds)
                {
                    DrawBoundGizmo(bound.min, bound.max);
                }
            }
        }
    }

    void DrawBoundGizmo(Vector2Int min, Vector2Int max)
    {
        Vector3 center = new Vector3((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0);
        Vector3 size = new Vector3(Mathf.Abs(max.x - min.x) + 1, Mathf.Abs(max.y - min.y) + 1, 1);
        Gizmos.DrawWireCube(center, size);
    }

    public void ClearCurrentLevel()
    {
        foreach (var b in boxes) if (b != null) Destroy(b.gameObject);
        boxes.Clear();

        foreach (var g in goals) if (g != null) Destroy(g.gameObject);
        goals.Clear();

        foreach (var d in doors) if (d != null) Destroy(d.gameObject);
        doors.Clear();

        foreach (var w in spawnedWalls) if (w != null) Destroy(w);
        spawnedWalls.Clear();

        traps.Clear();
    }

    public void SpawnLevel(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= levelGroups.Count) return;

        var group = levelGroups[groupIndex];

        if (groupIndex == 0 && playerInstance == null && group.playerSettings.useFixed)
        {
            SpawnPlayer(group.playerSettings);
        }

        if (group.wallSettings.useFixed) SpawnWalls(group.wallSettings);

        if (group.goalSettings.useFixed && group.goalSettings.fixedCount > 0)
            SpawnFixedGoals(group.goalSettings, group.randomSettings);
        if (group.goalSettings.useRandom && group.goalSettings.randomCount > 0)
            SpawnRandomGoals(group.goalSettings, group.randomSettings);

        if (group.doorSettings.useFixed && group.doorSettings.fixedCount > 0)
            SpawnFixedDoors(group.doorSettings, group.randomSettings, groupIndex);
        if (group.doorSettings.useRandom && group.doorSettings.randomCount > 0)
            SpawnRandomDoors(group.doorSettings, group.randomSettings, groupIndex);

        if (group.boxSettings.useFixed) SpawnFixedBoxes(group.boxSettings, group.randomSettings);
        if (group.boxSettings.useRandom) SpawnRandomBoxes(group.boxSettings, group.randomSettings);

        if (group.trapSettings.useFixed && group.trapSettings.fixedCount > 0)
            SpawnFixedTraps(group.trapSettings);
        if (group.trapSettings.useRandom && group.trapSettings.randomCount > 0)
            SpawnRandomTraps(group.trapSettings, group.randomSettings);
    }

    public void LoadLevelData()
    {
        ClearCurrentLevel();
    }

    public void SpawnObjects()
    {
        SpawnLevel(0);
    }

    #region Spawn Methods
    void SpawnPlayer(PlayerSettings settings)
    {
        playerInstance = Instantiate(playerPrefab, settings.spawnPoint, Quaternion.identity);
    }

    void SpawnWalls(WallSettings settings)
    {
        if (wallPrefab == null) return;
        foreach (var pos in settings.wallPositions)
        {
            GameObject w = Instantiate(wallPrefab, pos, Quaternion.identity);
            spawnedWalls.Add(w);
        }
    }

    void SpawnFixedGoals(GoalSettings settings, RandomSettings random)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var point in settings.fixedPoints)
            if (!occupied.Contains(point)) availablePoints.Add(point);

        ShuffleList(availablePoints);
        int spawnCount = Mathf.Min(settings.fixedCount, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = availablePoints[i];
            GameObject g = Instantiate(goalPrefab, pos, Quaternion.identity);
            goals.Add(g.GetComponent<Goal>());
        }
    }

    void SpawnRandomGoals(GoalSettings settings, RandomSettings random)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        for (int i = 0; i < settings.randomCount; i++)
        {
            Vector2 pos = GetRandomIntPosition(occupied, random);
            GameObject g = Instantiate(goalPrefab, pos, Quaternion.identity);
            goals.Add(g.GetComponent<Goal>());
            occupied.Add(pos);
        }
    }

    void SpawnFixedDoors(DoorSettings settings, RandomSettings random, int groupIndex)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var point in settings.fixedPoints)
            if (!occupied.Contains(point)) availablePoints.Add(point);

        ShuffleList(availablePoints);
        int spawnCount = Mathf.Min(settings.fixedCount, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = availablePoints[i];
            GameObject d = Instantiate(doorPrefab, pos, Quaternion.identity);

            Door doorComp = d.GetComponent<Door>();
            if (doorComp != null)
            {
                doorComp.CloseDoor();
                doorComp.isFinalDoor = (groupIndex == levelGroups.Count - 1);
                doors.Add(doorComp);
            }
        }
    }

    void SpawnRandomDoors(DoorSettings settings, RandomSettings random, int groupIndex)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        for (int i = 0; i < settings.randomCount; i++)
        {
            Vector2 pos = GetRandomIntPosition(occupied, random);
            GameObject d = Instantiate(doorPrefab, pos, Quaternion.identity);

            Door doorComp = d.GetComponent<Door>();
            if (doorComp != null)
            {
                doorComp.CloseDoor();
                doorComp.isFinalDoor = (groupIndex == levelGroups.Count - 1);
                doors.Add(doorComp);
                occupied.Add(pos);
            }
        }
    }

    void SpawnFixedBoxes(BoxSettings settings, RandomSettings random)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();

        foreach (var p in settings.spawnPoints)
            if (!occupied.Contains(p)) availablePoints.Add(p);

        ShuffleList(availablePoints);

        foreach (var req in settings.colorSpawnRequests)
        {
            for (int i = 0; i < req.count; i++)
            {
                if (availablePoints.Count == 0) return;
                Vector2 pos = availablePoints[0];
                availablePoints.RemoveAt(0);

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

    void SpawnRandomBoxes(BoxSettings settings, RandomSettings random)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        foreach (var boxInfo in settings.randomBoxes)
        {
            for (int i = 0; i < boxInfo.count; i++)
            {
                Vector2 pos = GetRandomIntPosition(occupied, random);
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

    void SpawnFixedTraps(TrapSettings settings)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        List<Vector2> availablePoints = new List<Vector2>();
        foreach (var point in settings.fixedPoints)
            if (!occupied.Contains(point)) availablePoints.Add(point);

        ShuffleList(availablePoints);
        int spawnCount = Mathf.Min(settings.fixedCount, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 pos = availablePoints[i];
            traps.Add(new TrapData(pos, trapPrefab));
        }
    }

    void SpawnRandomTraps(TrapSettings settings, RandomSettings random)
    {
        HashSet<Vector2> occupied = GetOccupiedPositions();
        for (int i = 0; i < settings.randomCount; i++)
        {
            Vector2 pos = GetRandomIntPosition(occupied, random);
            traps.Add(new TrapData(pos, trapPrefab));
            occupied.Add(pos);
        }
    }
    #endregion

    #region Utility
    HashSet<Vector2> GetOccupiedPositions()
    {
        HashSet<Vector2> occupied = new HashSet<Vector2>();
        if (goals != null) foreach (var g in goals) if (g != null) occupied.Add(g.transform.position);
        if (doors != null) foreach (var d in doors) if (d != null) occupied.Add(d.transform.position);
        if (boxes != null) foreach (var b in boxes) if (b != null) occupied.Add(b.transform.position);
        if (traps != null) foreach (var t in traps) if (t != null) occupied.Add(t.position);
        return occupied;
    }

    Vector2 GetRandomIntPosition(HashSet<Vector2> occupied, RandomSettings random)
    {
        Vector2 pos = Vector2.zero;
        int tries = 0;
        do
        {
            pos = new Vector2(
                Random.Range(random.randomMin.x, random.randomMax.x + 1),
                Random.Range(random.randomMin.y, random.randomMax.y + 1)
            );
            tries++;
        } while (occupied.Contains(pos) && tries < 50);
        return pos;
    }

    GameObject GetRandomPrefabOfColor(ColorType color)
    {
        List<GameObject> matched = new List<GameObject>();
        foreach (var p in boxColorPrefabs)
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

[System.Serializable]
public class BoundData
{
    public Vector2Int min;
    public Vector2Int max;
}

#region Settings & LevelGroup
[System.Serializable]
public class LevelGroup
{
    public string groupName;

    [Header("Map Bounds (Walkable Area)")]
    public Vector2Int boundMin = new Vector2Int(-10, -10);
    public Vector2Int boundMax = new Vector2Int(10, 10);

    [Header("Inner Obstacles (Non-Walkable Bounds)")]
    public List<BoundData> innerBounds;

    [Header("Player")]
    public PlayerSettings playerSettings;

    [Header("Walls")]
    public WallSettings wallSettings;

    [Header("Goals")]
    public GoalSettings goalSettings;

    [Header("Doors")]
    public DoorSettings doorSettings;

    [Header("Boxes")]
    public BoxSettings boxSettings;

    [Header("Traps")]
    public TrapSettings trapSettings;

    [Header("Random Settings")]
    public RandomSettings randomSettings;
}

[System.Serializable] public class PlayerSettings { public Vector2 spawnPoint = Vector2.zero; public bool useFixed = true; }
[System.Serializable] public class WallSettings { public List<Vector2> wallPositions; public bool useFixed = true; }
[System.Serializable] public class GoalSettings { public bool useFixed = true; public bool useRandom = false; public List<Vector2> fixedPoints; public int fixedCount = 1; public int randomCount = 3; }
[System.Serializable] public class DoorSettings { public bool useFixed = true; public bool useRandom = false; public List<Vector2> fixedPoints; public int fixedCount = 1; public int randomCount = 1; }
[System.Serializable] public class TrapSettings { public bool useFixed = true; public bool useRandom = false; public List<Vector2> fixedPoints; public int fixedCount = 1; public int randomCount = 2; }
[System.Serializable] public class BoxSettings { public List<Vector2> spawnPoints; public List<ColorSpawnRequest> colorSpawnRequests; public List<RandomBoxInfo> randomBoxes; public bool useFixed = true; public bool useRandom = false; }
[System.Serializable] public class RandomSettings { public Vector2Int randomMin = new Vector2Int(-5, -5); public Vector2Int randomMax = new Vector2Int(5, 5); }
#endregion

#region Data Classes
[System.Serializable] public class TrapData { public Vector2 position; public bool triggered = false; public GameObject prefab; public TrapData(Vector2 pos, GameObject p) { position = pos; prefab = p; triggered = false; } }
[System.Serializable] public class BoxColorPrefabPair { public GameObject prefab; public ColorType color; }
[System.Serializable] public class ColorSpawnRequest { public ColorType color; public int count; }
[System.Serializable] public class RandomBoxInfo { public ColorType color; public int count; }
#endregion