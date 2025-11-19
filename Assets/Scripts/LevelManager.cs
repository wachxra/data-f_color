using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;

    [Header("Box Spawn Settings")]
    public List<BoxSpawnInfo> boxSpawnInfos;

    [Header("Goal Spawn Settings")]
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
        Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        foreach (var info in goalSpawnInfos)
        {
            for (int i = 0; i < info.count; i++)
            {
                Vector2 pos = info.startPos + new Vector2(i * info.spacing.x, i * info.spacing.y);
                GameObject g = Instantiate(info.prefab, pos, Quaternion.identity);
                goals.Add(g.GetComponent<Goal>());
            }
        }

        foreach (var info in boxSpawnInfos)
        {
            for (int i = 0; i < info.count; i++)
            {
                Vector2 pos = info.startPos + new Vector2(i * info.spacing.x, i * info.spacing.y);
                GameObject b = Instantiate(info.prefab, pos, Quaternion.identity);

                Box box = b.GetComponent<Box>();
                box.colorType = info.color;
                box.levelManager = this;

                boxes.Add(box);
            }
        }
    }

    public GameObject GetMergeResult(ColorType a, ColorType b, out bool explode)
    {
        return mergeRule.GetResult(a, b, out explode);
    }
}

[System.Serializable]
public class BoxSpawnInfo
{
    public GameObject prefab;
    public ColorType color;
    public Vector2 startPos;
    public int count = 1;
    public Vector2 spacing = new Vector2(1, 0);
}

[System.Serializable]
public class GoalSpawnInfo
{
    public GameObject prefab;
    public Vector2 startPos;
    public int count = 1;
    public Vector2 spacing = new Vector2(1, 0);
}