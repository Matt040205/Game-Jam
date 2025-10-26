using System.Collections;
using UnityEngine;
using FMODUnity; // <- Adicionado

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("FMOD Events")]
    [Tooltip("Caminho do evento FMOD para o som do Dash.")]
    [EventRef] public string dashSoundEvent; // <- Substitui AudioClip

    // Remove: private AudioSource audioSource;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection, lastMoveDirection, dashDirection;
    private bool isMoving, isDashing, canDash = true, canMove = true;
    public bool invertControls = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // Remove: audioSource = GetComponent<AudioSource>();
        lastMoveDirection = Vector2.down;
    }

    void Update()
    {
        if (canMove)
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");
            if (invertControls) { inputX *= -1; inputY *= -1; }
            moveDirection = new Vector2(inputX, inputY).normalized;
            isMoving = moveDirection.magnitude > 0.1f;
            if (isMoving) lastMoveDirection = moveDirection;
        }
        else { moveDirection = Vector2.zero; isMoving = false; }

        if (Input.GetKeyDown(KeyCode.Space) && canDash) StartCoroutine(Dash());
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDashing) rb.linearVelocity = dashDirection * dashSpeed;
        else if (isMoving) rb.linearVelocity = moveDirection * moveSpeed;
        else rb.linearVelocity = Vector2.zero;
    }

    private void UpdateAnimations()
    {
        animator.SetBool("isMoving", isMoving);
        Vector2 visualDirection = isDashing ? dashDirection : lastMoveDirection;
        if (isMoving || isDashing)
        {
            animator.SetFloat("MoveX", visualDirection.x);
            animator.SetFloat("MoveY", visualDirection.y);
            if (visualDirection.x < -0.1f) spriteRenderer.flipX = true;
            else if (visualDirection.x > 0.1f) spriteRenderer.flipX = false;
        }
    }

    private IEnumerator Dash()
    {
        canDash = false; isDashing = true; canMove = false;
        if (moveDirection.magnitude > 0.1f) dashDirection = moveDirection;
        else dashDirection = lastMoveDirection;
        lastMoveDirection = dashDirection;

        // --- FMOD ---
        if (!string.IsNullOrEmpty(dashSoundEvent))
        {
            RuntimeManager.PlayOneShot(dashSoundEvent, transform.position);
        }
        // --- FIM FMOD ---

        animator.SetTrigger("Dash");
        yield return new WaitForSeconds(dashDuration);
        isDashing = false; canMove = true;
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        canDash = true;
    }

    public Vector2 GetLastMoveDirection() { return lastMoveDirection; }
}