using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public Vector2 gridSize = new Vector2(5, 5);
    public float tileSpacing = 1f;

    [HideInInspector] public List<Box> boxes = new List<Box>();
    [HideInInspector] public List<Goal> goals = new List<Goal>();

    public GameManager gameManager;

    public void LoadLevelData()
    {
        boxes.Clear();
        goals.Clear();
    }

    public void SpawnObjects()
    {
        Instantiate(playerPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        Vector2[] goalPositions = { new Vector2(2, 2), new Vector2(3, 1) };
        foreach (var pos in goalPositions)
        {
            GameObject g = Instantiate(goalPrefab, pos, Quaternion.identity);
            goals.Add(g.GetComponent<Goal>());
        }

        Vector2[] boxPositions = { new Vector2(1, 2), new Vector2(3, 3) };
        foreach (var pos in boxPositions)
        {
            GameObject b = Instantiate(boxPrefab, pos, Quaternion.identity);
            boxes.Add(b.GetComponent<Box>());
        }
    }
}