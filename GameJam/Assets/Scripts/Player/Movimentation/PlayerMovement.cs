using System.Collections;
using UnityEngine;
using UnityEngine.UI; // <-- **ADICIONAR** para Image
using FMODUnity;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f; // Cooldown between dashes

    // --- **NOVO:** Stamina Section ---
    [Header("Stamina")]
    public float maxStamina = 100f;
    private float currentStamina;
    public float dashStaminaCost = 35f; // How much stamina a dash consumes
    public float staminaRegenRate = 20f; // Stamina points per second
    public float staminaRegenDelay = 1.0f; // Delay after dash before regen starts
    private float lastDashTime = -10f; // Track when the last dash occurred
    // --- **FIM NOVO** ---

    // --- **NOVO:** UI Reference ---
    [Header("UI")]
    [Tooltip("Arraste a Image da barra de stamina (com Image Type = Filled) para cá.")]
    public Image staminaBarImage;
    // --- **FIM NOVO** ---

    [Header("FMOD Events")]
    [EventRef] public string dashSoundEvent;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection, lastMoveDirection, dashDirection;
    private bool isMoving, isDashing = false, canDash = true, canMove = true;
    public bool invertControls = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        lastMoveDirection = Vector2.down;

        // **NOVO:** Initialize Stamina & UI
        currentStamina = maxStamina;
        UpdateStaminaBar();
        // **FIM NOVO**
    }

    void Update()
    {
        // Movement Input
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

        // Dash Input Check
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDashing) // Added !isDashing check
        {
            // --- **NOVO:** Stamina Check ---
            if (currentStamina >= dashStaminaCost)
            {
                StartCoroutine(Dash());
            }
            else
            {
                Debug.Log("[PlayerMovement] Sem stamina suficiente para o dash!");
                // Opcional: Tocar um som de "falha" aqui
            }
            // --- **FIM NOVO** ---
        }

        // --- **NOVO:** Stamina Regeneration ---
        // Regenerate only if not currently dashing and after the regen delay
        if (!isDashing && Time.time > lastDashTime + staminaRegenDelay)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); // Clamp to max
                UpdateStaminaBar(); // Update UI during regen
            }
        }
        // --- **FIM NOVO** ---

        UpdateAnimations();
    }

    void FixedUpdate()
    {
        // --- **CORREÇÃO FÍSICA** ---
        if (isDashing) rb.linearVelocity = dashDirection * dashSpeed;
        else if (isMoving) rb.linearVelocity = moveDirection * moveSpeed;
        else rb.linearVelocity = Vector2.zero;
        // --- **FIM CORREÇÃO** ---
    }

    // --- MODIFICADO: Lógica de Animação e Flip ---
    private void UpdateAnimations()
    {
        if (animator == null || spriteRenderer == null) return;

        // --- Lógica do Animator (Exemplo) ---
        animator.SetBool("isMoving", isMoving);

        // Atualiza o "blend tree" de movimento (se você tiver um)
        if (isMoving)
        {
            animator.SetFloat("MoveX", moveDirection.x);
            animator.SetFloat("MoveY", moveDirection.y);
        }
        else if (!isDashing) // Se parado e sem dash, usa a última direção
        {
            animator.SetFloat("MoveX", lastMoveDirection.x);
            animator.SetFloat("MoveY", lastMoveDirection.y);
        }
        // --- Fim Lógica Animator ---


        // --- Lógica do Flip (Inverter Sprite) ---
        float horizontalDirection = 0f;

        if (isDashing)
        {
            horizontalDirection = dashDirection.x; // Usa a direção do dash
        }
        else if (isMoving)
        {
            horizontalDirection = moveDirection.x; // Usa a direção do input
        }
        // Se (horizontalDirection == 0), ele mantém o flip anterior

        // Inverte a sprite com base na direção horizontal
        if (horizontalDirection > 0.01f)
        {
            spriteRenderer.flipX = false; // Olhando para a direita
        }
        else if (horizontalDirection < -0.01f)
        {
            spriteRenderer.flipX = true; // Olhando para a esquerda (invertido)
        }
        // --- Fim Lógica do Flip ---
    }
    // --- FIM DA MODIFICAÇÃO ---

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        canMove = false;
        lastDashTime = Time.time; // Record dash time for regen delay

        // --- **NOVO:** Consume Stamina ---
        currentStamina -= dashStaminaCost;
        UpdateStaminaBar(); // Update UI after consuming
        Debug.Log($"[PlayerMovement] Dash! Stamina restante: {currentStamina}");
        // --- **FIM NOVO** ---

        if (moveDirection.magnitude > 0.1f) dashDirection = moveDirection;
        else dashDirection = lastMoveDirection;
        lastMoveDirection = dashDirection;

        if (!string.IsNullOrEmpty(dashSoundEvent))
        {
            RuntimeManager.PlayOneShot(dashSoundEvent, transform.position);
        }

        animator.SetTrigger("Dash");
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        canMove = true;

        yield return new WaitForSeconds(dashCooldown); // Use dashCooldown here
        canDash = true;
    }

    // --- **NOVO:** Update Stamina Bar Function ---
    private void UpdateStaminaBar()
    {
        if (staminaBarImage != null)
        {
            staminaBarImage.fillAmount = currentStamina / maxStamina;
        }
        // else Debug.LogWarning("[PlayerMovement] Stamina Bar Image não definida!"); // Opcional
    }
    // --- **FIM NOVO** ---

    public Vector2 GetLastMoveDirection() { return lastMoveDirection; }
}