using UnityEngine;

public class TileObject : MonoBehaviour
{
    public Vector2Int gridPos;

    private void OnValidate()
    {
        if (gridPos.x < 0 || gridPos.y < 0)
        {
            Debug.LogWarning($"[TileObject] Invalid gridPos {gridPos} on {name}. Auto-corrected to (0,0).");
            gridPos = Vector2Int.zero;
        }
    }
}