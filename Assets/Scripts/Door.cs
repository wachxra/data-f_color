using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door Status")]
    public bool isOpen = false;
    public bool isFinalDoor = false;

    public Vector2 nextSpawnPosition;

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }

    public void OpenDoor()
    {
        isOpen = true;
        UpdateVisuals();
    }

    public void CloseDoor()
    {
        isOpen = false;
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (sr != null)
        {
            sr.color = isOpen ? Color.green : Color.red;
        }
    }

    public void Enter(GameObject player)
    {
        if (!isOpen) return;

        GameManager gm = FindFirstObjectByType<GameManager>();

        if (isFinalDoor)
        {
            if (gm != null) gm.WinGame();
        }
        else
        {
            if (gm != null) gm.AdvanceLevel();
        }
    }
}