using UnityEngine;

public class Goal : TileObject
{
    public bool isCompleted = false;

    public void Initialize(Vector2Int pos)
    {
        gridPos = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    public void OnBoxPlaced(Box b)
    {
        if (b.colorType != ColorType.Black)
            return;

        isCompleted = true;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.green;

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            gm.CheckWinCondition();
        }
    }

    public void OnBoxRemoved()
    {
        if (isCompleted)
        {
            isCompleted = false;

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.gray;

            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.CheckWinCondition();
            }
        }
    }
}