using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private bool isMoving;

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
        Collider2D hit = Physics2D.OverlapPoint(targetPos);

        if (hit == null)
        {
            StartCoroutine(MoveTo(targetPos));
        }
        else if (hit.CompareTag("Box"))
        {
            Box box = hit.GetComponent<Box>();
            Vector2 boxTarget = targetPos + direction;
            Collider2D boxHit = Physics2D.OverlapPoint(boxTarget);

            if (boxHit == null || boxHit.CompareTag("Box"))
            {
                box.Move(direction);
                StartCoroutine(MoveTo(targetPos));
            }
        }
    }

    System.Collections.IEnumerator MoveTo(Vector2 target)
    {
        isMoving = true;
        while ((target - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
        isMoving = false;
    }
}