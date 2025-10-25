using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveDirection;
    private Vector2 lastMoveDirection;
    private Vector2 dashDirection;
    private bool isMoving = false;
    private bool isDashing = false;
    private bool canDash = true;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        lastMoveDirection = Vector2.down;
    }

    void Update()
    {
        if (canMove)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            moveDirection = new Vector2(moveX, moveY).normalized;
            isMoving = moveDirection.magnitude > 0.1f;

            if (isMoving)
            {
                lastMoveDirection = moveDirection;
            }
        }
        else
        {
            moveDirection = Vector2.zero;
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartCoroutine(Dash());
        }

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else if (isMoving)
        {
            rb.linearVelocity = moveDirection * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void UpdateAnimations()
    {
        animator.SetBool("isMoving", isMoving);

        Vector2 visualDirection = isDashing ? dashDirection : lastMoveDirection;

        if (isMoving || isDashing)
        {
            animator.SetFloat("MoveX", visualDirection.x);
            animator.SetFloat("MoveY", visualDirection.y);

            if (visualDirection.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else if (visualDirection.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        canMove = false;

        if (moveDirection.magnitude > 0.1f)
        {
            dashDirection = moveDirection;
        }
        else
        {
            dashDirection = lastMoveDirection;
        }

        lastMoveDirection = dashDirection;

        animator.SetTrigger("Dash");

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        canMove = true;

        yield return new WaitForSeconds(dashCooldown - dashDuration);

        canDash = true;
    }

    public Vector2 GetLastMoveDirection()
    {
        return lastMoveDirection;
    }
}