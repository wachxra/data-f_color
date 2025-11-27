using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private bool isMoving;
    private LevelManager levelManager;
    private GameManager gameManager;

    private void Awake()
    {
        levelManager = Object.FindFirstObjectByType<LevelManager>();
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        Move();
    }

    void Move()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W)) TryMove(Vector2.up);
            if (Input.GetKeyDown(KeyCode.S)) TryMove(Vector2.down);
            if (Input.GetKeyDown(KeyCode.A)) TryMove(Vector2.left);
            if (Input.GetKeyDown(KeyCode.D)) TryMove(Vector2.right);
        }
    }

    void TryMove(Vector2 direction)
    {
        Vector2 targetPos = (Vector2)transform.position + direction;

        if (gameManager != null && !gameManager.IsPositionInsideBounds(targetPos))
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapPoint(targetPos);

        if (hit == null)
        {
            StartCoroutine(MoveTo(targetPos));
        }
        else if (hit.CompareTag("Box"))
        {
            Box box = hit.GetComponent<Box>();
            if (box == null) return;

            Vector2 boxTargetPos = targetPos + direction;
            if (gameManager != null && !gameManager.IsPositionInsideBounds(boxTargetPos))
            {
                return;
            }

            bool boxMoved = box.TryMoveBox(direction, true);

            if (boxMoved)
            {
                StartCoroutine(MoveTo(targetPos));
            }
        }
        else if (hit.CompareTag("Door"))
        {
            Door door = hit.GetComponent<Door>();
            if (door != null && door.isOpen)
            {
                door.Enter(gameObject);
            }
        }
    }

    IEnumerator MoveTo(Vector2 target)
    {
        isMoving = true;
        while ((target - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;

        CheckForTrap();

        isMoving = false;
    }

    void CheckForTrap()
    {
        if (levelManager == null || levelManager.traps == null) return;

        Vector2 playerPos = transform.position;

        foreach (var trap in levelManager.traps)
        {
            if (!trap.triggered && Vector2.Distance(trap.position, playerPos) < 0.1f)
            {
                trap.triggered = true;

                if (trap.prefab != null)
                {
                    GameObject trapObj = Instantiate(trap.prefab, trap.position, Quaternion.identity);
                    Box trapBox = trapObj.GetComponent<Box>();
                    if (trapBox != null)
                    {
                        trapBox.levelManager = levelManager;
                        trapBox.gridPos = Vector2Int.RoundToInt(trap.position);

                        trapBox.isBlocked = true;
                        trapBox.StartCoroutine(trapBox.BlinkThenExplode(3f));
                    }
                }

                break;
            }
        }
    }
}