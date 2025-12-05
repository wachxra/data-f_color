using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    private int width = 5;
    private int height = 5;
    public TileObject[,] grid;

    void Awake()
    {
        grid = new TileObject[width, height];
    }

    public void RegisterObject(TileObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("[GridManager] RegisterObject called with NULL object");
            return;
        }

        Vector2Int pos = obj.gridPos;

        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
        {
            Debug.LogError($"[GridManager] Object '{obj.name}' has invalid gridPos {pos}");
            return;
        }

        if (grid[pos.x, pos.y] != null)
        {
            Debug.LogWarning($"[GridManager] Position {pos} already has object '{grid[pos.x, pos.y].name}'. Registration blocked.");
            return;
        }

        grid[pos.x, pos.y] = obj;
    }

    public TileObject GetObjectAt(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return null;

        return grid[pos.x, pos.y];
    }

    public bool CanMoveTo(Vector2Int pos)
    {
        return GetObjectAt(pos) == null;
    }
}