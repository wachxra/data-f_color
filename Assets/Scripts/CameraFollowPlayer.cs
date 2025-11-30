using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;

    [Header("Follow Settings")]
    public bool followPlayerAlways = false;
    public float smoothSpeed = 5f;

    [Header("Camera Bounds")]
    public Vector2 minBound;
    public Vector2 maxBound;

    [Header("Initial Camera Position")]
    public bool useInitialPosition = false;
    public Vector2 initialPosition;

    private Camera cam;
    private bool waitingForPlayer = false;

    void Start()
    {
        cam = Camera.main;

        if (useInitialPosition && !followPlayerAlways)
        {
            Vector3 pos = new Vector3(initialPosition.x, initialPosition.y, transform.position.z);
            pos.x = Mathf.Clamp(pos.x, minBound.x, maxBound.x);
            pos.y = Mathf.Clamp(pos.y, minBound.y, maxBound.y);
            transform.position = pos;
            waitingForPlayer = true;
        }

        FindPlayerIfNeeded();
    }

    void LateUpdate()
    {
        if (player == null)
        {
            FindPlayerIfNeeded();
            return;
        }

        if (waitingForPlayer)
        {
            waitingForPlayer = false;
        }

        if (!waitingForPlayer)
        {
            if (followPlayerAlways)
            {
                Vector3 targetPos = new Vector3(player.position.x, player.position.y, transform.position.z);

                transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 pos = player.position;
                pos.z = transform.position.z;

                pos.x = Mathf.Clamp(pos.x, minBound.x, maxBound.x);
                pos.y = Mathf.Clamp(pos.y, minBound.y, maxBound.y);

                transform.position = pos;
            }
        }
    }

    void FindPlayerIfNeeded()
    {
        if (player != null) return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;

            if (followPlayerAlways)
            {
                Vector3 startPos = new Vector3(player.position.x, player.position.y, transform.position.z);
                transform.position = startPos;
            }
        }
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBound = min;
        maxBound = max;
    }
}