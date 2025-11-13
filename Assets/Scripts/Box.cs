using UnityEngine;

public class Box : TileObject
{
    private Goal currentGoal;

    public void Initialize(Vector2Int pos)
    {
        gridPos = pos;
        transform.position = new Vector3(pos.x, pos.y, 0);
    }

    public void Move(Vector2 direction)
    {
        Vector2 target = (Vector2)transform.position + direction;
        transform.position = target;

        CheckGoalStatus();
    }

    private void CheckGoalStatus()
    {
        Goal newGoal = null;
        Goal[] allGoals = FindObjectsByType<Goal>(FindObjectsSortMode.None);

        foreach (Goal g in allGoals)
        {
            if (Vector2.Distance(g.transform.position, transform.position) < 0.1f)
            {
                newGoal = g;
                break;
            }
        }

        if (currentGoal != newGoal)
        {
            if (currentGoal != null)
                currentGoal.OnBoxRemoved();

            if (newGoal != null)
                newGoal.OnBoxPlaced(this);

            currentGoal = newGoal;
        }
    }
}